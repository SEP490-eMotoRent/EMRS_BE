// EMRS.Application/Services/RentalReceiptService.cs
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.RentalReceiptDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EMRS.Application.Services;

public class RentalReceiptService : IRentalReceiptService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IGeminiAIService _geminiAIService;
    private readonly IFacePlusPlusClient _facePlusPlusClient;

    public RentalReceiptService(
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
            var rentalReceipt = activeBooking.RentalReceipt;
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
                return ResultResponse<UploadReturnImagesResponse>.Failure(
                    "Booking not found");
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

            // 3. Upload return images lên Cloudinary
            var uploadedUrls = new List<string>();
            var imageSides = new[] { "front", "back", "left", "right" };

            for (int i = 0; i < request.ReturnImages.Count; i++)
            {
                var imageFile = request.ReturnImages[i];
                var imageSide = imageSides[i];

                var url = await _cloudinaryService.UploadImageFileAsync(
                    imageFile,
                    $"return_{imageSide}_{Generator.PublicIdGenerate()}_{DateTime.Now:yyyyMMddHHmmss}",
                    "RentalReceipt/Return"
                );

                if (url == null)
                {
                    return ResultResponse<UploadReturnImagesResponse>.Failure(
                        $"Failed to upload {imageSide} image");
                }

                uploadedUrls.Add(url);

                // Lưu vào Media
                var media = new Media
                {
                    FileUrl = url,
                    DocNo = booking.RentalReceipt.Id,
                    EntityType = MediaEntityTypeEnum.RentalReceiptReturnImage.ToString(),
                    MediaType = MediaTypeEnum.Image.ToString()
                };
                await _unitOfWork.GetMediaRepository().AddAsync(media);
            }

            await _unitOfWork.SaveChangesAsync();

            // 4. Lấy ảnh handover để so sánh
            var handoverImages = await _unitOfWork.GetMediaRepository()
                .GetMediaByDocNoAndTypeAsync(
                    booking.RentalReceipt.Id,
                    MediaEntityTypeEnum.RentalReceiptHandoverImage.ToString()
                );

            var handoverUrls = handoverImages.Select(m => m.FileUrl).ToList();

            if (handoverUrls.Count == 0)
            {
                return ResultResponse<UploadReturnImagesResponse>.Failure(
                    "Handover images not found");
            }

            // 5. Gọi Gemini AI để phân tích (OPTIONAL - không block workflow)
            VehicleVerificationResult verificationResult;
            DamageDetectionResult damageResult;

            try
            {
                // 5a. Xác thực xe
                verificationResult = await _geminiAIService.VerifyVehicleAsync(
                    handoverUrls,
                    uploadedUrls,
                    booking.Vehicle.LicensePlate
                );

                // 5b. Phát hiện hư hỏng
                damageResult = await _geminiAIService.DetectDamagesAsync(
                    handoverUrls,
                    uploadedUrls
                );
            }
            catch (Exception aiEx)
            {
                // AI fail không block workflow
                Console.WriteLine($"AI Analysis failed: {aiEx.Message}");

                verificationResult = new VehicleVerificationResult
                {
                    IsVerified = false,
                    Confidence = 0,
                    Reason = $"AI analysis failed: {aiEx.Message}. Staff manual verification required.",
                    LicensePlateMatch = "UNCLEAR"
                };

                damageResult = new DamageDetectionResult
                {
                    HasNewDamages = false,
                    Suggestions = new List<DamageSuggestion>()
                };
            }

            // 6. Cập nhật odometer và battery vào RentalReceipt
            var rentalReceipt = booking.RentalReceipt;
            rentalReceipt.EndOdometerKm = request.EndOdometerKm;
            rentalReceipt.EndBatteryPercentage = request.EndBatteryPercentage;

            _unitOfWork.GetRentalReceiptRepository().Update(rentalReceipt);
            await _unitOfWork.SaveChangesAsync();

            // 7. Chuẩn bị response
            var response = new UploadReturnImagesResponse
            {
                UploadedImageUrls = uploadedUrls,
                VerificationResult = verificationResult,
                DamageResult = damageResult
            };

            return ResultResponse<UploadReturnImagesResponse>.SuccessResult(
                "Images uploaded and analyzed successfully", response);
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

            // 1. Lấy booking với tất cả thông tin
            var booking = await _unitOfWork.GetBookingRepository()
                .GetBookingForSettlementAsync(request.BookingId);

            if (booking == null)
            {
                return ResultResponse<CreateReturnReceiptResponse>.Failure(
                    "Booking not found");
            }

            // 2. Cập nhật RentalReceipt
            var rentalReceipt = booking.RentalReceipt;
            rentalReceipt.EndOdometerKm = request.EndOdometerKm;
            rentalReceipt.EndBatteryPercentage = request.EndBatteryPercentage;
            rentalReceipt.Notes = request.Notes;

            _unitOfWork.GetRentalReceiptRepository().Update(rentalReceipt);

            // 3. Upload checklist return (nếu có)
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

            // 4. Tạo AdditionalFees (Staff nhập thủ công)
            if (request.AdditionalFees != null && request.AdditionalFees.Count > 0)
            {
                foreach (var feeInput in request.AdditionalFees)
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

            // 5. TÍNH TOÁN SETTLEMENT
            var settlement = await CalculateSettlementAsync(booking);

            // 6. Cập nhật booking với tổng chi phí
            booking.TotalAdditionalFee = settlement.TotalAdditionalFees;
            booking.TotalChargingFee = settlement.TotalChargingFee;
            booking.TotalAmount = settlement.TotalAmount;
            booking.RefundAmount = settlement.RefundAmount;

            // Tách các phí chi tiết
            booking.LateReturnFee = settlement.FeesBreakdown.LateReturnFee;
            booking.CleaningFee = settlement.FeesBreakdown.CleaningFee;
            booking.CrossBranchFee = settlement.FeesBreakdown.CrossBranchFee;
            booking.ExcessKmFee = settlement.FeesBreakdown.ExcessKmFee;

            _unitOfWork.GetBookingRepository().Update(booking);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            // 7. Chuẩn bị response
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

            // 3. Lấy wallet của renter
            var wallet = await _unitOfWork.GetWalletRepository()
                .GetWalletByAccountIdAsync(booking.Renter.AccountId);

            if (wallet == null)
            {
                return ResultResponse<FinalizeReturnResponse>.Failure(
                    "Wallet not found");
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

            // 6. CẬP NHẬT XÁC NHẬN CỦA RENTER
            if (request.RenterConfirmed)
            {
                booking.RentalReceipt.RenterConfirmedAt = DateTime.UtcNow;
            }

            // 7. CẬP NHẬT TRẠNG THÁI XE
            var vehicle = booking.Vehicle;
            vehicle.Status = VehicleStatusEnum.Available.ToString();
            vehicle.CurrentOdometerKm = booking.RentalReceipt.EndOdometerKm;
            vehicle.BatteryHealthPercentage = booking.RentalReceipt.EndBatteryPercentage;

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
}