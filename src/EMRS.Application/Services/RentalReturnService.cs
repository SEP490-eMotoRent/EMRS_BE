// EMRS.Application/Services/RentalReceiptService.cs
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.RentalReceiptDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EMRS.Application.Services;

public class RentalReturnService : IRentalReturnService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IGeminiAIService _geminiAIService;
    private readonly IFacePlusPlusClient _facePlusPlusClient;

    public RentalReturnService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ICloudinaryService cloudinaryService,
        IGeminiAIService geminiAIService,
        IFacePlusPlusClient facePlusPlusClient)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _cloudinaryService = cloudinaryService;
        _geminiAIService = geminiAIService;
        _facePlusPlusClient = facePlusPlusClient;
    }

    // ========================================================================
    // API 1: SCAN FACE VÀ KHỞI TẠO QUY TRÌNH RETURN
    // ========================================================================
    public async Task<ResultResponse<ReturnInitResponse>> InitiateReturnProcessAsync(
        IFormFile faceImage)
    {
        try
        {
            // 1. Scan face và xác thực renter
            var config = await _unitOfWork.GetConfigurationRepository()
                .Query()
                .FirstOrDefaultAsync(c => c.Type == (int)ConfigurationTypeEnum.FacePlusPlus);

            if (config == null)
            {
                return ResultResponse<ReturnInitResponse>.Failure(
                    "Face recognition configuration not found");
            }

            var faceSearchResult = await _facePlusPlusClient.SearchByFileAsync(
                faceImage, config.Value);

            if (faceSearchResult == null)
            {
                return ResultResponse<ReturnInitResponse>.Failure(
                    "Face recognition failed");
            }

            // 2. Tìm renter theo face token
            var renter = await _unitOfWork.GetRenterRepository()
                .Query()
                .Include(r => r.Account)
                .FirstOrDefaultAsync(r => r.FaceToken == faceSearchResult.Id);

            if (renter == null)
            {
                return ResultResponse<ReturnInitResponse>.Failure(
                    "Renter not found with this face");
            }

            // 3. Upload face scan image
            var faceScanUrl = await _cloudinaryService.UploadImageFileAsync(
                faceImage,
                $"facescan_{Generator.PublicIdGenerate()}_{DateTime.Now:yyyyMMddHHmmss}",
                "FaceScan"
            );

            if (faceScanUrl == null)
            {
                return ResultResponse<ReturnInitResponse>.Failure(
                    "Failed to upload face scan image");
            }

            // 4. Lưu face scan vào Media
            var faceScanMedia = new Media
            {
                FileUrl = faceScanUrl,
                DocNo = renter.Id,
                EntityType = MediaEntityTypeEnum.RenterFaceScan.ToString(),
                MediaType = MediaTypeEnum.Image.ToString()
            };
            await _unitOfWork.GetMediaRepository().AddAsync(faceScanMedia);
            await _unitOfWork.SaveChangesAsync();

            // 5. Tìm booking đang active của renter
            var activeBooking = await _unitOfWork.GetBookingRepository()
                .GetActiveBookingByRenterIdAsync(renter.Id);

            if (activeBooking == null)
            {
                return ResultResponse<ReturnInitResponse>.Failure(
                    "No active booking found for this renter");
            }

            // 6. Lấy thông tin rental receipt (handover)
            var rentalReceipt = activeBooking.RentalReceipts.OrderByDescending(r => r.CreatedAt).FirstOrDefault();
            if (rentalReceipt == null)
            {
                return ResultResponse<ReturnInitResponse>.Failure(
                    "Rental receipt not found. Vehicle handover might not be completed.");
            }

            // 7. Lấy ảnh handover từ Media
            var handoverImages = await _unitOfWork.GetMediaRepository()
                .GetMediaByDocNoAndTypeAsync(
                    rentalReceipt.Id,
                    MediaEntityTypeEnum.RentalReceiptHandoverImage.ToString()
                );

            var handoverChecklist = await _unitOfWork.GetMediaRepository()
                .Query()
                .Where(m => m.DocNo == rentalReceipt.Id
                    && m.EntityType == MediaEntityTypeEnum.RentalReceiptCheckListHandOver.ToString())
                .FirstOrDefaultAsync();

            // 8. Chuẩn bị response
            var response = new ReturnInitResponse
            {
                BookingId = activeBooking.Id,
                RenterId = renter.Id,
                RenterName = renter.Account.Fullname,
                RenterPhone = renter.phone,
                RenterEmail = renter.Email,
                FaceScanUrl = faceScanUrl,

                VehicleId = activeBooking.VehicleId ?? Guid.Empty,
                LicensePlate = activeBooking.Vehicle?.LicensePlate ?? "N/A",
                VehicleModelName = activeBooking.Vehicle?.VehicleModel?.ModelName ?? "N/A",
                VehicleColor = activeBooking.Vehicle?.Color ?? "N/A",

                RentalReceiptId = rentalReceipt.Id,
                StartOdometerKm = rentalReceipt.StartOdometerKm,
                StartBatteryPercentage = rentalReceipt.StartBatteryPercentage,
                HandoverTime = rentalReceipt.CreatedAt,
                HandoverImageUrls = handoverImages.Select(m => m.FileUrl).ToList(),
                HandoverChecklistUrl = handoverChecklist?.FileUrl,

                StartDatetime = activeBooking.StartDatetime ?? DateTimeOffset.UtcNow,
                EndDatetime = activeBooking.EndDatetime ?? DateTimeOffset.UtcNow,
                DepositAmount = activeBooking.DepositAmount,
                TotalRentalFee = activeBooking.TotalRentalFee
            };

            return ResultResponse<ReturnInitResponse>.SuccessResult(
                "Return process initiated successfully", response);
        }
        catch (Exception ex)
        {
            return ResultResponse<ReturnInitResponse>.Failure(
                $"An error occurred: {ex.Message}");
        }
    }

    // ========================================================================
    // API 2: UPLOAD ẢNH RETURN VÀ PHÂN TÍCH AI
    // ========================================================================
    public async Task<ResultResponse<UploadReturnImagesResponse>> UploadAndAnalyzeReturnImagesAsync(
    UploadReturnImagesRequest request)
    {
        try
        {
            // 1. Validate booking
            var booking = await _unitOfWork.GetBookingRepository()
                .GetBookingForSettlementAsync(request.BookingId);

            if (booking == null)
            {
                return ResultResponse<UploadReturnImagesResponse>.Failure("Booking not found");
            }

            if (booking.Vehicle == null)
            {
                return ResultResponse<UploadReturnImagesResponse>.Failure(
                    "Vehicle not assigned to this booking");
            }

            // 2. Validate return images count
            if (request.ReturnImages == null || request.ReturnImages.Count != 4)
            {
                return ResultResponse<UploadReturnImagesResponse>.Failure(
                    "Exactly 4 return images are required (front, back, left, right)");
            }

            // 3. Upload ảnh lên Cloudinary (Trả Urls về để lưu DB)
            var uploadedUrls = new List<string>();
            var imageSides = new[] { "front", "back", "left", "right" };

            for (int i = 0; i < request.ReturnImages.Count; i++)
            {
                var imageFile = request.ReturnImages[i];
                var imageSide = imageSides[i];

                var url = await _cloudinaryService.UploadImageFileAsync(
                    imageFile,
                    $"temp_return_{imageSide}_{Generator.PublicIdGenerate()}_{DateTime.Now:yyyyMMddHHmmss}",
                    "RentalReceipt/Return" 
                );

                if (url == null)
                {
                    return ResultResponse<UploadReturnImagesResponse>.Failure(
                        $"Failed to upload {imageSide} image");
                }

                uploadedUrls.Add(url);

            }

            var rentalReceipt = booking.RentalReceipts.OrderByDescending(r => r.CreatedAt).FirstOrDefault();

            if (rentalReceipt == null)
            {
                return ResultResponse<UploadReturnImagesResponse>.Failure(
                    "Rental receipt not found");
            }

            // 4. Lấy ảnh handover để so sánh
            var handoverImages = await _unitOfWork.GetMediaRepository()
                .GetMediaByDocNoAndTypeAsync(
                    rentalReceipt.Id,
                    MediaEntityTypeEnum.RentalReceiptHandoverImage.ToString()
                );

            var handoverUrls = handoverImages.Select(m => m.FileUrl).ToList();

            if (handoverUrls.Count == 0)
            {
                return ResultResponse<UploadReturnImagesResponse>.Failure(
                    "Handover images not found");
            }

            // 5. Gọi Gemini AI để phân tích
            var verificationResult = await _geminiAIService.VerifyVehicleAsync(
                handoverUrls,
                uploadedUrls,
                booking.Vehicle.LicensePlate
            );

            var damageResult = await _geminiAIService.DetectDamagesAsync(
                handoverUrls,
                uploadedUrls
            );


            // 7. Response
            var response = new UploadReturnImagesResponse
            {
                UploadedImageUrls = uploadedUrls,  // ← Frontend lưu tạm URLs này
                VerificationResult = verificationResult,
                DamageResult = damageResult
            };

            return ResultResponse<UploadReturnImagesResponse>.SuccessResult(
                "Images uploaded and analyzed successfully. Note: Images are temporary and will be saved when creating receipt.",
                response);
        }
        catch (Exception ex)
        {
            return ResultResponse<UploadReturnImagesResponse>.Failure(
                $"An error occurred: {ex.Message}");
        }
    }

    // ========================================================================
    // API 3: TẠO BIÊN BẢN TRẢ XE VỚI CHI PHÍ
    // ========================================================================
    public async Task<ResultResponse<CreateReturnReceiptResponse>> CreateReturnReceiptAsync(
        CreateReturnReceiptRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // 1. Validate
            var booking = await _unitOfWork.GetBookingRepository()
                .GetBookingForSettlementAsync(request.BookingId);

            if (booking == null)
            {
                return ResultResponse<CreateReturnReceiptResponse>.Failure("Booking not found");
            }

            // ===== BƯỚC 2: DESERIALIZE ReturnImageUrls =====
            List<string> imageUrls = new List<string>();

            if (!string.IsNullOrEmpty(request.ReturnImageUrls))
            {

                try
                {
                    imageUrls = JsonSerializer.Deserialize<List<string>>(request.ReturnImageUrls);

                    if (imageUrls == null || !imageUrls.Any())
                    {
                        return ResultResponse<CreateReturnReceiptResponse>.Failure(
                            "Return image URLs are required. Please upload images first using API 2.");
                    }
                }
                catch (JsonException ex)
                {
                    return ResultResponse<CreateReturnReceiptResponse>.Failure(
                        $"Invalid JSON format for ReturnImageUrls: {ex.Message}");
                }
            }
            else
            {
                return ResultResponse<CreateReturnReceiptResponse>.Failure(
                    "ReturnImageUrls is required. Expected format: [\"url1\",\"url2\",\"url3\",\"url4\"]");
            }

            // 3. Cập nhật RentalReceipt
            var rentalReceipt = booking.RentalReceipts.OrderByDescending(r => r.CreatedAt).FirstOrDefault();

            if (rentalReceipt == null)
            {
                return ResultResponse<CreateReturnReceiptResponse>.Failure(
                    "Rental receipt not found. Handover process might not be completed.");
            }

            rentalReceipt.EndOdometerKm = request.EndOdometerKm;
            rentalReceipt.EndBatteryPercentage = request.EndBatteryPercentage;
            rentalReceipt.Notes = request.Notes;

            _unitOfWork.GetRentalReceiptRepository().Update(rentalReceipt);

            // 4. ✅ LƯU ẢNH RETURN VÀO DB (từ URLs đã upload ở API 2)
            foreach (var imageUrl in imageUrls)
            {
                var media = new Media
                {
                    FileUrl = imageUrl,
                    DocNo = rentalReceipt.Id,
                    EntityType = MediaEntityTypeEnum.RentalReceiptReturnImage.ToString(),
                    MediaType = MediaTypeEnum.Image.ToString()
                };
                await _unitOfWork.GetMediaRepository().AddAsync(media);
            }

            // 5. Upload checklist (nếu có)
            if (request.ChecklistImage != null)
            {
                var checklistUrl = await _cloudinaryService.UploadImageFileAsync(
                    request.ChecklistImage,
                    $"checklist_return_{Generator.PublicIdGenerate()}_{DateTime.Now:yyyyMMddHHmmss}",
                    "RentalReceipt/Checklist"
                );

                if (checklistUrl != null)
                {
                    var checklistMedia = new Media
                    {
                        FileUrl = checklistUrl,
                        DocNo = rentalReceipt.Id,
                        EntityType = MediaEntityTypeEnum.RentalReceiptCheckListReturn.ToString(),
                        MediaType = MediaTypeEnum.Image.ToString()
                    };
                    await _unitOfWork.GetMediaRepository().AddAsync(checklistMedia);
                }
            }

            // ===== BƯỚC 6: DESERIALIZE AdditionalFees =====
            List<AdditionalFeeInput> additionalFeeInputs = new List<AdditionalFeeInput>();

            if (!string.IsNullOrEmpty(request.AdditionalFees))
            {
                try
                {
                    additionalFeeInputs = JsonSerializer.Deserialize<List<AdditionalFeeInput>>(
                        request.AdditionalFees,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                }
                catch (JsonException ex)
                {
                    return ResultResponse<CreateReturnReceiptResponse>.Failure(
                        $"Invalid JSON format for AdditionalFees: {ex.Message}");
                }
            }

            // 7. Tạo AdditionalFees (nếu có)
            if (additionalFeeInputs != null && additionalFeeInputs.Count > 0)
            {
                foreach (var feeInput in additionalFeeInputs)
                {
                    var fee = new AdditionalFee
                    {
                        BookingId = request.BookingId,
                        FeeType = feeInput.FeeType,
                        Description = feeInput.Description,
                        Amount = feeInput.Amount
                    };
                    await _unitOfWork.GetAdditionalFeeRepository().AddAsync(fee);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            // 8. Tính settlement
            var settlement = await CalculateSettlementAsync(booking);

            // 9. Cập nhật booking
            booking.TotalAdditionalFee = settlement.TotalAdditionalFees;
            booking.TotalChargingFee = settlement.TotalChargingFee;
            booking.TotalAmount = settlement.TotalAmount;
            booking.RefundAmount = settlement.RefundAmount;
            booking.LateReturnFee = settlement.FeesBreakdown.LateReturnFee;
            booking.CleaningFee = settlement.FeesBreakdown.CleaningFee;
            booking.CrossBranchFee = settlement.FeesBreakdown.CrossBranchFee;
            booking.ExcessKmFee = settlement.FeesBreakdown.ExcessKmFee;

            _unitOfWork.GetBookingRepository().Update(booking);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            // 10. Response
            var response = new CreateReturnReceiptResponse
            {
                BookingId = booking.Id,
                RentalReceiptId = rentalReceipt.Id,
                Settlement = settlement
            };

            return ResultResponse<CreateReturnReceiptResponse>.SuccessResult(
                "Return receipt created successfully", response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<CreateReturnReceiptResponse>.Failure(
                $"An error occurred: {ex.Message}");
        }
    }

    // ========================================================================
    // API 4: HOÀN TẤT TRẢ XE VÀ THANH TOÁN
    // ========================================================================
    public async Task<ResultResponse<FinalizeReturnResponse>> FinalizeReturnAsync(
        FinalizeReturnRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // 1. Lấy booking
            var booking = await _unitOfWork.GetBookingRepository()
                .GetBookingForSettlementAsync(request.BookingId);

            if (booking == null)
            {
                return ResultResponse<FinalizeReturnResponse>.Failure(
                    "Booking not found");
            }

            // 2. Kiểm tra booking status
            if (booking.BookingStatus != BookingStatusEnum.Renting.ToString())
            {
                return ResultResponse<FinalizeReturnResponse>.Failure(
                    $"Booking is not in valid status for return. Current status: {booking.BookingStatus}");
            }

            // ✅ ĐÚNG: Dùng RenterId thay vì AccountId
            var wallet = await _unitOfWork.GetWalletRepository()
                .GetWalletByRenterIdAsync(booking.RenterId);

            if (wallet == null)
            {
                return ResultResponse<FinalizeReturnResponse>.Failure("Wallet not found");
            }

            // 4. XỬ LÝ THANH TOÁN
            PaymentResult paymentResult;

            if (booking.RefundAmount > 0)
            {
                // HOÀN TIỀN
                wallet.Balance += booking.RefundAmount;

                var refundTransaction = new Transaction
                {
                    TransactionType = ((int)TransactionTypeEnum.BookingRefund).ToString(),
                    Amount = booking.RefundAmount,
                    DocNo = booking.Id,
                    Status = TransactionStatusEnum.Success.ToString()
                };
                await _unitOfWork.GetTransactionRepository().AddAsync(refundTransaction);

                paymentResult = new PaymentResult
                {
                    RefundAmount = booking.RefundAmount,
                    TransactionType = "REFUND",
                    WalletBalanceAfter = wallet.Balance
                };
            }
            else if (booking.RefundAmount < 0)
            {
                                    // KHÁCH PHẢI TRẢ THÊM
                var additionalPayment = Math.Abs(booking.RefundAmount);

                if (wallet.Balance < additionalPayment)
                {
                    await _unitOfWork.RollbackAsync();
                    return ResultResponse<FinalizeReturnResponse>.Failure(
                        $"Insufficient wallet balance. Required: {additionalPayment:N0} VND, Available: {wallet.Balance:N0} VND");
                }

                wallet.Balance -= additionalPayment;

                var paymentTransaction = new Transaction
                {
                    TransactionType = ((int)TransactionTypeEnum.BookingFinalPayment).ToString(),
                    Amount = additionalPayment,
                    DocNo = booking.Id,
                    Status = TransactionStatusEnum.Success.ToString()
                };
                await _unitOfWork.GetTransactionRepository().AddAsync(paymentTransaction);

                paymentResult = new PaymentResult
                {
                    RefundAmount = booking.RefundAmount,
                    TransactionType = "ADDITIONAL_PAYMENT",
                    WalletBalanceAfter = wallet.Balance
                };
            }
            else
            {
                // RefundAmount = 0 (không hoàn, không trả thêm)
                paymentResult = new PaymentResult
                {
                    RefundAmount = 0,
                    TransactionType = "NO_TRANSACTION",
                    WalletBalanceAfter = wallet.Balance
                };
            }

            // 5. CẬP NHẬT TRẠNG THÁI BOOKING
            booking.ActualReturnDatetime = DateTime.UtcNow;
            booking.BookingStatus = BookingStatusEnum.Completed.ToString();

            var rentalReceipt = booking.RentalReceipts
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefault();

            if (rentalReceipt == null)
            {
                return ResultResponse<FinalizeReturnResponse>.Failure(
                    "Rental receipt not found");
            }

            // 6. CẬP NHẬT XÁC NHẬN CỦA RENTER
            if (request.RenterConfirmed)
            {
                rentalReceipt.RenterConfirmedAt = DateTime.UtcNow;
                _unitOfWork.GetRentalReceiptRepository().Update(rentalReceipt);
            }

            // 7. CẬP NHẬT TRẠNG THÁI XE
            var vehicle = booking.Vehicle;
            vehicle.Status = VehicleStatusEnum.Available.ToString();
            vehicle.CurrentOdometerKm = rentalReceipt.EndOdometerKm;
            vehicle.BatteryHealthPercentage = rentalReceipt.EndBatteryPercentage; 

            var vehicleUpdate = new VehicleStatusUpdate
            {
                VehicleId = vehicle.Id,
                Status = vehicle.Status,
                CurrentOdometerKm = vehicle.CurrentOdometerKm,
                BatteryHealthPercentage = vehicle.BatteryHealthPercentage
            };

            // 8. LƯU TẤT CẢ THAY ĐỔI
            _unitOfWork.GetBookingRepository().Update(booking);
            _unitOfWork.GetVehicleRepository().Update(vehicle);
            _unitOfWork.GetWalletRepository().Update(wallet);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            // 9. CHUẨN BỊ RESPONSE
            var response = new FinalizeReturnResponse
            {
                BookingId = booking.Id,
                BookingStatus = booking.BookingStatus,
                ActualReturnDatetime = booking.ActualReturnDatetime ?? DateTimeOffset.UtcNow,
                PaymentResult = paymentResult,
                VehicleUpdate = vehicleUpdate
            };

            return ResultResponse<FinalizeReturnResponse>.SuccessResult(
                "Return finalized successfully", response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<FinalizeReturnResponse>.Failure(
                $"An error occurred: {ex.Message}");
        }
    }

    // ========================================================================
    // API 5: LẤY TÓM TẮT QUYẾT TOÁN
    // ========================================================================
    public async Task<ResultResponse<SettlementSummary>> GetSettlementSummaryAsync(Guid bookingId)
    {
        try
        {
            var booking = await _unitOfWork.GetBookingRepository()
                .GetBookingForSettlementAsync(bookingId);

            if (booking == null)
            {
                return ResultResponse<SettlementSummary>.Failure("Booking not found");
            }

            var settlement = await CalculateSettlementAsync(booking);

            return ResultResponse<SettlementSummary>.SuccessResult(
                "Settlement summary retrieved successfully", settlement);
        }
        catch (Exception ex)
        {
            return ResultResponse<SettlementSummary>.Failure(
                $"An error occurred: {ex.Message}");
        }
    }

    // ========================================================================
    // HELPER METHOD: TÍNH TOÁN SETTLEMENT
    // ========================================================================
    private async Task<SettlementSummary> CalculateSettlementAsync(Booking booking)
    {
        // 1. Base rental fee (đã tính từ lúc booking)
        var baseRentalFee = booking.TotalRentalFee;

        // 2. Total charging fee
        var totalChargingFee = booking.ChargingRecords?.Sum(c => c.Fee) ?? 0;

        // 3. Additional fees breakdown
        var additionalFees = booking.AdditionalFees?.ToList() ?? new List<AdditionalFee>();

        var damageFee = additionalFees
            .Where(f => f.FeeType == "DAMAGE")
            .Sum(f => f.Amount);

        var cleaningFee = additionalFees
            .Where(f => f.FeeType == "CLEANING")
            .Sum(f => f.Amount);

        var lateReturnFee = additionalFees
            .Where(f => f.FeeType == "LATE_RETURN")
            .Sum(f => f.Amount);

        var crossBranchFee = additionalFees
            .Where(f => f.FeeType == "CROSS_BRANCH")
            .Sum(f => f.Amount);

        var excessKmFee = additionalFees
            .Where(f => f.FeeType == "EXCESS_KM")
            .Sum(f => f.Amount);

        var totalAdditionalFees = damageFee + cleaningFee + lateReturnFee + crossBranchFee + excessKmFee;

        // 4. Total amount
        var totalAmount = baseRentalFee + totalChargingFee + totalAdditionalFees;

        // 5. Refund amount (positive = hoàn tiền, negative = phải trả thêm)
        var refundAmount = booking.DepositAmount - totalAmount;

        // 6. Tạo settlement summary
        return new SettlementSummary
        {
            BaseRentalFee = baseRentalFee,
            TotalChargingFee = totalChargingFee,
            TotalAdditionalFees = totalAdditionalFees,
            FeesBreakdown = new AdditionalFeesBreakdown
            {
                DamageFee = damageFee,
                CleaningFee = cleaningFee,
                LateReturnFee = lateReturnFee,
                CrossBranchFee = crossBranchFee,
                ExcessKmFee = excessKmFee
            },
            TotalAmount = totalAmount,
            DepositAmount = booking.DepositAmount,
            RefundAmount = refundAmount
        };
    }

    public async Task<ResultResponse<UpdateReturnReceiptResponse>> UpdateReturnReceiptAsync(
    UpdateReturnReceiptRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // ===== BƯỚC 1: VALIDATE BOOKING =====
            var booking = await _unitOfWork.GetBookingRepository()
                .GetBookingForSettlementAsync(request.BookingId);

            if (booking == null)
            {
                return ResultResponse<UpdateReturnReceiptResponse>.Failure("Booking not found");
            }

            // Chỉ cho phép update nếu booking chưa Completed
            if (booking.BookingStatus == BookingStatusEnum.Completed.ToString())
            {
                return ResultResponse<UpdateReturnReceiptResponse>.Failure(
                    "Cannot update return receipt for completed booking");
            }

            var rentalReceipt = booking.RentalReceipts
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefault();

            if (rentalReceipt == null)
            {
                return ResultResponse<UpdateReturnReceiptResponse>.Failure(
                    "Rental receipt not found");
            }

            if (booking.Vehicle == null)
            {
                return ResultResponse<UpdateReturnReceiptResponse>.Failure(
                    "Vehicle not found");
            }

            var updateSummary = new UpdateSummary();

            // ===== BƯỚC 2: CẬP NHẬT ODOMETER (NẾU CÓ) =====
            if (request.EndOdometerKm.HasValue)
            {
                // Validate
                if (request.EndOdometerKm.Value < rentalReceipt.StartOdometerKm)
                {
                    return ResultResponse<UpdateReturnReceiptResponse>.Failure(
                        $"End odometer ({request.EndOdometerKm.Value} km) cannot be less than start odometer ({rentalReceipt.StartOdometerKm} km)");
                }

                rentalReceipt.EndOdometerKm = request.EndOdometerKm.Value;
                updateSummary.OdometerUpdated = true;
            }

            // ===== BƯỚC 3: CẬP NHẬT BATTERY (NẾU CÓ) =====
            if (request.EndBatteryPercentage.HasValue)
            {
                // Validate
                if (request.EndBatteryPercentage.Value < 0 || request.EndBatteryPercentage.Value > 100)
                {
                    return ResultResponse<UpdateReturnReceiptResponse>.Failure(
                        "Battery percentage must be between 0 and 100");
                }

                rentalReceipt.EndBatteryPercentage = request.EndBatteryPercentage.Value;
                updateSummary.BatteryUpdated = true;
            }

            // ===== BƯỚC 4: CẬP NHẬT NOTES (NẾU CÓ) =====
            if (!string.IsNullOrEmpty(request.Notes))
            {
                rentalReceipt.Notes = request.Notes;
                updateSummary.NotesUpdated = true;
            }

            // ===== BƯỚC 5: THAY THẾ ẢNH RETURN (NẾU CÓ) =====
            if (request.NewReturnImages != null && request.NewReturnImages.Any())
            {
                // 5a. XÓA TẤT CẢ ảnh cũ
                var oldReturnImages = await _unitOfWork.GetMediaRepository()
                    .GetMediaByDocNoAndTypeAsync(
                        rentalReceipt.Id,
                        MediaEntityTypeEnum.RentalReceiptReturnImage.ToString()
                    );

                foreach (var oldImage in oldReturnImages)
                {
                    // Xóa từ Cloudinary
                    await _cloudinaryService.DeleteImageFileByUrlAsync(
                        oldImage.FileUrl,
                        "RentalReceipt/Return"
                    );

                    // Xóa khỏi database
                    _unitOfWork.GetMediaRepository().Delete(oldImage);
                }

                // 5b. UPLOAD ảnh mới
                var newImageUrls = new List<string>();
                var imageSides = new[] { "front", "back", "left", "right" };

                for (int i = 0; i < request.NewReturnImages.Count; i++)
                {
                    var imageFile = request.NewReturnImages[i];
                    var imageSide = i < imageSides.Length ? imageSides[i] : $"extra_{i}";

                    var url = await _cloudinaryService.UploadImageFileAsync(
                        imageFile,
                        $"return_{imageSide}_{Generator.PublicIdGenerate()}_{DateTime.Now:yyyyMMddHHmmss}",
                        "RentalReceipt/Return"
                    );

                    if (url != null)
                    {
                        newImageUrls.Add(url);

                        // Lưu vào Media
                        var media = new Media
                        {
                            FileUrl = url,
                            DocNo = rentalReceipt.Id,
                            EntityType = MediaEntityTypeEnum.RentalReceiptReturnImage.ToString(),
                            MediaType = MediaTypeEnum.Image.ToString()
                        };
                        await _unitOfWork.GetMediaRepository().AddAsync(media);
                    }
                }

                updateSummary.ImagesReplaced = true;
                updateSummary.NewImagesCount = newImageUrls.Count;
            }

            await _unitOfWork.SaveChangesAsync();

            // ===== BƯỚC 6: CHẠY LẠI AI ANALYSIS (NẾU YÊU CẦU) =====
            VehicleVerificationResult? newVerificationResult = null;
            DamageDetectionResult? newDamageResult = null;

            if (request.RerunAIAnalysis)
            {
                // Lấy ảnh return mới nhất
                var currentReturnImages = await _unitOfWork.GetMediaRepository()
                    .GetMediaByDocNoAndTypeAsync(
                        rentalReceipt.Id,
                        MediaEntityTypeEnum.RentalReceiptReturnImage.ToString()
                    );

                var returnUrls = currentReturnImages.Select(m => m.FileUrl).ToList();

                // Lấy ảnh handover
                var handoverImages = await _unitOfWork.GetMediaRepository()
                    .GetMediaByDocNoAndTypeAsync(
                        rentalReceipt.Id,
                        MediaEntityTypeEnum.RentalReceiptHandoverImage.ToString()
                    );

                var handoverUrls = handoverImages.Select(m => m.FileUrl).ToList();

                if (returnUrls.Any() && handoverUrls.Any())
                {
                    // AI Verification
                    newVerificationResult = await _geminiAIService.VerifyVehicleAsync(
                        handoverUrls,
                        returnUrls,
                        booking.Vehicle.LicensePlate
                    );

                    // AI Damage Detection
                    newDamageResult = await _geminiAIService.DetectDamagesAsync(
                        handoverUrls,
                        returnUrls
                    );

                    updateSummary.AIAnalysisRerun = true;
                }
            }

            // ===== BƯỚC 7: THAY THẾ CHECKLIST (NẾU CÓ) =====
            if (request.NewChecklistImage != null)
            {
                // 7a. XÓA checklist cũ
                var oldChecklist = await _unitOfWork.GetMediaRepository()
                    .Query()
                    .Where(m => m.DocNo == rentalReceipt.Id
                        && m.EntityType == MediaEntityTypeEnum.RentalReceiptCheckListReturn.ToString())
                    .FirstOrDefaultAsync();

                if (oldChecklist != null)
                {
                    await _cloudinaryService.DeleteImageFileByUrlAsync(
                        oldChecklist.FileUrl,
                        "RentalReceipt/Checklist"
                    );
                    _unitOfWork.GetMediaRepository().Delete(oldChecklist);
                }

                // 7b. UPLOAD checklist mới
                var checklistUrl = await _cloudinaryService.UploadImageFileAsync(
                    request.NewChecklistImage,
                    $"checklist_return_{Generator.PublicIdGenerate()}_{DateTime.Now:yyyyMMddHHmmss}",
                    "RentalReceipt/Checklist"
                );

                if (checklistUrl != null)
                {
                    var checklistMedia = new Media
                    {
                        FileUrl = checklistUrl,
                        DocNo = rentalReceipt.Id,
                        EntityType = MediaEntityTypeEnum.RentalReceiptCheckListReturn.ToString(),
                        MediaType = MediaTypeEnum.Image.ToString()
                    };
                    await _unitOfWork.GetMediaRepository().AddAsync(checklistMedia);
                    updateSummary.ChecklistReplaced = true;
                }
            }

            // ===== BƯỚC 8: THAY THẾ CHI PHÍ (NẾU CÓ) =====
            if (request.NewAdditionalFees != null && request.NewAdditionalFees.Any())
            {
                // 8a. XÓA TẤT CẢ phí cũ
                var oldFees = await _unitOfWork.GetAdditionalFeeRepository()
                    .GetAdditionalFeesByBookingIdAsync(request.BookingId);

                foreach (var oldFee in oldFees)
                {
                    _unitOfWork.GetAdditionalFeeRepository().Delete(oldFee);
                }

                // 8b. THÊM phí mới
                foreach (var feeInput in request.NewAdditionalFees)
                {
                    var newFee = new AdditionalFee
                    {
                        BookingId = request.BookingId,
                        FeeType = feeInput.FeeType,
                        Description = feeInput.Description,
                        Amount = feeInput.Amount
                    };
                    await _unitOfWork.GetAdditionalFeeRepository().AddAsync(newFee);
                }

                updateSummary.AdditionalFeesReplaced = true;
            }

            // ===== BƯỚC 9: LƯU THAY ĐỔI =====
            _unitOfWork.GetRentalReceiptRepository().Update(rentalReceipt);
            await _unitOfWork.SaveChangesAsync();

            // ===== BƯỚC 10: TÍNH LẠI SETTLEMENT =====
            // Reload booking để có data mới nhất
            await _unitOfWork.GetBookingRepository()
                .Query()
                .Where(b => b.Id == request.BookingId)
                .Include(b => b.ChargingRecords)
                .Include(b => b.AdditionalFees)
                .LoadAsync(); // ← Load các navigation properties mới

            var newSettlement = await CalculateSettlementAsync(booking);

            // Cập nhật booking với settlement mới
            booking.TotalAdditionalFee = newSettlement.TotalAdditionalFees;
            booking.TotalChargingFee = newSettlement.TotalChargingFee;
            booking.TotalAmount = newSettlement.TotalAmount;
            booking.RefundAmount = newSettlement.RefundAmount;
            booking.LateReturnFee = newSettlement.FeesBreakdown.LateReturnFee;
            booking.CleaningFee = newSettlement.FeesBreakdown.CleaningFee;
            booking.CrossBranchFee = newSettlement.FeesBreakdown.CrossBranchFee;
            booking.ExcessKmFee = newSettlement.FeesBreakdown.ExcessKmFee;

            _unitOfWork.GetBookingRepository().Update(booking);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            // ===== BƯỚC 11: RESPONSE =====
            var response = new UpdateReturnReceiptResponse
            {
                BookingId = booking.Id,
                RentalReceiptId = rentalReceipt.Id,
                UpdateSummary = updateSummary,
                NewSettlement = newSettlement,
                NewVerificationResult = newVerificationResult,
                NewDamageResult = newDamageResult
            };

            return ResultResponse<UpdateReturnReceiptResponse>.SuccessResult(
                "Return receipt updated successfully", response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<UpdateReturnReceiptResponse>.Failure(
                $"An error occurred: {ex.Message}");
        }
    }

    // ========================================================================
    // API 7: XÓA BIÊN BẢN TRẢ XE
    // ========================================================================
    public async Task<ResultResponse<string>> DeleteReturnReceiptAsync(Guid bookingId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // 1. Lấy booking
            var booking = await _unitOfWork.GetBookingRepository()
                .GetBookingForSettlementAsync(bookingId);

            if (booking == null)
            {
                return ResultResponse<string>.Failure("Booking not found");
            }

            // 2. Chỉ cho phép xóa nếu booking chưa Completed
            if (booking.BookingStatus == BookingStatusEnum.Completed.ToString())
            {
                return ResultResponse<string>.Failure(
                    "Cannot delete return receipt for completed booking");
            }

            var rentalReceipt = booking.RentalReceipts .OrderByDescending(r => r.CreatedAt).FirstOrDefault();

            if (rentalReceipt == null)
            {
                return ResultResponse<string>.Failure("Rental receipt not found");
            }
            if (rentalReceipt == null)
            {
                return ResultResponse<string>.Failure("Rental receipt not found");
            }

            // 3. XÓA ẢNH RETURN từ Cloudinary
            var returnImages = await _unitOfWork.GetMediaRepository()
                .GetMediaByDocNoAndTypeAsync(
                    rentalReceipt.Id,
                    MediaEntityTypeEnum.RentalReceiptReturnImage.ToString()
                );

            foreach (var image in returnImages)
            {
                await _cloudinaryService.DeleteImageFileByUrlAsync(
                    image.FileUrl,
                    "RentalReceipt/Return"
                );
                _unitOfWork.GetMediaRepository().Delete(image);
            }

            // 4. XÓA CHECKLIST RETURN từ Cloudinary
            var checklistReturn = await _unitOfWork.GetMediaRepository()
                .Query()
                .Where(m => m.DocNo == rentalReceipt.Id
                    && m.EntityType == MediaEntityTypeEnum.RentalReceiptCheckListReturn.ToString())
                .ToListAsync();

            foreach (var checklist in checklistReturn)
            {
                await _cloudinaryService.DeleteImageFileByUrlAsync(
                    checklist.FileUrl,
                    "RentalReceipt/Checklist"
                );
                _unitOfWork.GetMediaRepository().Delete(checklist);
            }

            // 5. XÓA ADDITIONAL FEES
            var additionalFees = await _unitOfWork.GetAdditionalFeeRepository()
                .GetAdditionalFeesByBookingIdAsync(bookingId);

            foreach (var fee in additionalFees)
            {
                _unitOfWork.GetAdditionalFeeRepository().Delete(fee);
            }

            // 6. RESET RentalReceipt về trạng thái chưa trả xe
            rentalReceipt.EndOdometerKm = 0;
            rentalReceipt.EndBatteryPercentage = 0;
            rentalReceipt.Notes = null;
            rentalReceipt.RenterConfirmedAt = null;

            _unitOfWork.GetRentalReceiptRepository().Update(rentalReceipt);

            // 7. RESET Booking settlement về 0
            booking.TotalAdditionalFee = 0;
            booking.TotalAmount = booking.TotalRentalFee + booking.TotalChargingFee;
            booking.RefundAmount = booking.DepositAmount - booking.TotalAmount;
            booking.LateReturnFee = 0;
            booking.CleaningFee = 0;
            booking.CrossBranchFee = 0;
            booking.ExcessKmFee = 0;
            booking.ActualReturnDatetime = null;

            // Chuyển booking về trạng thái Renting
            booking.BookingStatus = BookingStatusEnum.Renting.ToString();

            _unitOfWork.GetBookingRepository().Update(booking);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return ResultResponse<string>.SuccessResult(
                "Return receipt deleted successfully. Booking reset to Renting status.",
                null);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<string>.Failure($"An error occurred: {ex.Message}");
        }
    }
}