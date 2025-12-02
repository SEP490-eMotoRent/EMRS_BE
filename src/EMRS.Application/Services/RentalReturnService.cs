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
    private readonly IFacePlusPlusService _facePlusPlusClient;

    public RentalReturnService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ICloudinaryService cloudinaryService,
        IGeminiAIService geminiAIService,
        IFacePlusPlusService facePlusPlusClient)
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
          /*  var config = await _unitOfWork.GetConfigurationRepository()
                .Query()
                .FirstOrDefaultAsync(c => c.Type == (int)ConfigurationTypeEnum.FacePlusPlus);

            if (config == null)
            {
                return ResultResponse<ReturnInitResponse>.Failure(
                    "Face recognition configuration not found");
            }*/

            var faceSearchResult = await _facePlusPlusClient.SearchByFileAsync(
                faceImage);

            if (faceSearchResult == null)
            {
                return ResultResponse<ReturnInitResponse>.Failure(
                    "Face recognition failed");
            }

            
            var renter = await _unitOfWork.GetRenterRepository()
                .Query()
                .Include(r => r.Account)
                .FirstOrDefaultAsync(r => r.FaceToken == faceSearchResult.Id);

            if (renter == null)
            {
                return ResultResponse<ReturnInitResponse>.Failure(
                    "Renter not found with this face");
            }

            
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

            
            var faceScanMedia = new Media
            {
                FileUrl = faceScanUrl,
                DocNo = renter.Id,
                EntityType = MediaEntityTypeEnum.RenterFaceScan.ToString(),
                MediaType = MediaTypeEnum.Image.ToString()
            };
            await _unitOfWork.GetMediaRepository().AddAsync(faceScanMedia);
            await _unitOfWork.SaveChangesAsync();

            
            var activeBooking = await _unitOfWork.GetBookingRepository()
                .GetActiveBookingByRenterIdAsync(renter.Id);

            if (activeBooking == null)
            {
                return ResultResponse<ReturnInitResponse>.Failure(
                    "No active booking found for this renter");
            }

            
            var rentalReceipt = activeBooking.RentalReceipts.OrderByDescending(r => r.CreatedAt).FirstOrDefault();
            if (rentalReceipt == null)
            {
                return ResultResponse<ReturnInitResponse>.Failure(
                    "Rental receipt not found. Vehicle handover might not be completed.");
            }

            
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

    public async Task<ResultResponse<UploadReturnImagesResponse>> UploadAndAnalyzeReturnImagesAsync(
    UploadReturnImagesRequest request)
    {
        try
        {
            
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

            
            if (request.ReturnImages == null || request.ReturnImages.Count != 4)
            {
                return ResultResponse<UploadReturnImagesResponse>.Failure(
                    "Exactly 4 return images are required (front, back, left, right)");
            }

            
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

            
            var verificationResult = await _geminiAIService.VerifyVehicleAsync(
                handoverUrls,
                uploadedUrls,
                booking.Vehicle.LicensePlate
            );

            var damageResult = await _geminiAIService.DetectDamagesAsync(
                handoverUrls,
                uploadedUrls
            );


            
            var response = new UploadReturnImagesResponse
            {
                UploadedImageUrls = uploadedUrls,  
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


    public async Task<ResultResponse<CreateReturnReceiptResponse>> CreateReturnReceiptAsync(
    CreateReturnReceiptRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            
            var booking = await _unitOfWork.GetBookingRepository()
                .GetBookingForSettlementAsync(request.BookingId);

            if (booking == null)
            {
                return ResultResponse<CreateReturnReceiptResponse>.Failure("Booking not found");
            }

            if (booking.BookingStatus != BookingStatusEnum.Renting.ToString())
            {
                return ResultResponse<CreateReturnReceiptResponse>.Failure(
                    $"Booking must be in Renting status. Current: {booking.BookingStatus}");
            }

            
            if (request.ActualReturnDatetime < booking.StartDatetime)
            {
                return ResultResponse<CreateReturnReceiptResponse>.Failure(
                    $"ActualReturnDatetime cannot be before StartDatetime ({booking.StartDatetime})");
            }

            if (request.ActualReturnDatetime > DateTimeOffset.UtcNow.AddHours(1))
            {
                return ResultResponse<CreateReturnReceiptResponse>.Failure(
                    "ActualReturnDatetime cannot be in the future");
            }

            
            List<string> imageUrls = new List<string>();

            if (!string.IsNullOrEmpty(request.ReturnImageUrls))
            {
                try
                {
                    imageUrls = JsonSerializer.Deserialize<List<string>>(request.ReturnImageUrls);

                    if (imageUrls == null || !imageUrls.Any())
                    {
                        return ResultResponse<CreateReturnReceiptResponse>.Failure(
                            "Return image URLs are required.");
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
                    "ReturnImageUrls is required.");
            }

           
            var rentalReceipt = booking.RentalReceipts
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefault();

            if (rentalReceipt == null)
            {
                return ResultResponse<CreateReturnReceiptResponse>.Failure(
                    "Rental receipt not found. Handover might not be completed.");
            }

            if (rentalReceipt.EndOdometerKm > 0)
            {
                return ResultResponse<CreateReturnReceiptResponse>.Failure(
                    "Return receipt already exists. Use UpdateReturnReceipt API to modify.");
            }

            
            rentalReceipt.EndOdometerKm = request.EndOdometerKm;
            rentalReceipt.EndBatteryPercentage = request.EndBatteryPercentage;
            rentalReceipt.Notes = request.Notes;

            _unitOfWork.GetRentalReceiptRepository().Update(rentalReceipt);

            
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

            
            var staffId = Guid.Parse(_currentUserService.UserId);
            var staff = await _unitOfWork.GetStaffRepository()
                .Query()
                .Include(s => s.Branch)
                .FirstOrDefaultAsync(s => s.Id == staffId);

            if (staff == null || staff.BranchId == null)
            {
                return ResultResponse<CreateReturnReceiptResponse>.Failure(
                    "Staff branch not found");
            }

            
            booking.ActualReturnDatetime = request.ActualReturnDatetime;
            booking.ReturnBranchId = staff.BranchId;
            booking.BookingStatus = BookingStatusEnum.Returned.ToString();

            _unitOfWork.GetBookingRepository().Update(booking);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

           
            var settlement = await CalculateSettlementAsync(booking);

           
            var response = new CreateReturnReceiptResponse
            {
                BookingId = booking.Id,
                RentalReceiptId = rentalReceipt.Id,
                Settlement = settlement
            };

            return ResultResponse<CreateReturnReceiptResponse>.SuccessResult(
                "Return receipt created successfully. You can now add additional fees using AdditionalFee APIs.",
                response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<CreateReturnReceiptResponse>.Failure(
                $"An error occurred: {ex.Message}");
        }
    }


    public async Task<ResultResponse<FinalizeReturnResponse>> FinalizeReturnAsync(
    FinalizeReturnRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            
            var booking = await _unitOfWork.GetBookingRepository()
                .GetBookingForSettlementAsync(request.BookingId);

            if (booking == null)
            {
                return ResultResponse<FinalizeReturnResponse>.Failure("Booking not found");
            }

            
            if (booking.BookingStatus != BookingStatusEnum.Returned.ToString())
            {
                return ResultResponse<FinalizeReturnResponse>.Failure(
                    $"Booking is not in valid status for finalize. Current status: {booking.BookingStatus}");
            }

            
            var rentalReceipt = booking.RentalReceipts
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefault();

            if (rentalReceipt == null || rentalReceipt.EndOdometerKm == 0)
            {
                return ResultResponse<FinalizeReturnResponse>.Failure(
                    "Return receipt not created yet. Please create return receipt first.");
            }

            
            var wallet = await _unitOfWork.GetWalletRepository()
                .GetWalletByRenterIdAsync(booking.RenterId);

            if (wallet == null)
            {
                return ResultResponse<FinalizeReturnResponse>.Failure("Wallet not found");
            }

            
            var settlement = await CalculateSettlementAsync(booking);

            
            booking.TotalAdditionalFee = settlement.TotalAdditionalFees;
            booking.TotalChargingFee = settlement.TotalChargingFee;
            booking.TotalAmount = settlement.TotalAmount;
            booking.RefundAmount = settlement.RefundAmount;
            booking.LateReturnFee = settlement.FeesBreakdown.LateReturnFee;
            booking.CleaningFee = settlement.FeesBreakdown.CleaningFee;
            booking.CrossBranchFee = settlement.FeesBreakdown.CrossBranchFee;
            booking.ExcessKmFee = settlement.FeesBreakdown.ExcessKmFee;

            
            PaymentResult paymentResult = null!;

            if (booking.RefundAmount >= 0)
            {
                // HOÀN TIỀN
                wallet.Balance += booking.RefundAmount;

                var refundTransaction = new Transaction
                {
                    TransactionType = ((int)TransactionTypeEnum.BookingReturnRefund).ToString(),
                    Amount = booking.RefundAmount,
                    DocNo = booking.Id,
                    Status = TransactionStatusEnum.Success.ToString()
                };
                await _unitOfWork.GetTransactionRepository().AddAsync(refundTransaction);

                paymentResult = new PaymentResult
                {
                    RefundAmount = booking.RefundAmount,
                    TransactionType = ((int)TransactionTypeEnum.BookingReturnRefund).ToString(),
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
                    TransactionType = ((int)TransactionTypeEnum.BookingAdditionalPayment).ToString(),
                    Amount = additionalPayment,
                    DocNo = booking.Id,
                    Status = TransactionStatusEnum.Success.ToString()
                };
                await _unitOfWork.GetTransactionRepository().AddAsync(paymentTransaction);

                paymentResult = new PaymentResult
                {
                    RefundAmount = booking.RefundAmount,
                    TransactionType = ((int)TransactionTypeEnum.BookingAdditionalPayment).ToString(),
                    WalletBalanceAfter = wallet.Balance
                };
            }


            
            booking.BookingStatus = BookingStatusEnum.Completed.ToString();

            
            if (request.RenterConfirmed)
            {
                rentalReceipt.RenterConfirmedAt = DateTime.UtcNow;
                _unitOfWork.GetRentalReceiptRepository().Update(rentalReceipt);
            }

            
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

            
            _unitOfWork.GetBookingRepository().Update(booking);
            _unitOfWork.GetVehicleRepository().Update(vehicle);
            _unitOfWork.GetWalletRepository().Update(wallet);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            
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


    private async Task<SettlementSummary> CalculateSettlementAsync(Booking booking)
    {
        
        var totalChargingFee = booking.ChargingRecords?.Sum(c => c.Fee) ?? 0;

        
        var additionalFees = booking.AdditionalFees?.ToList() ?? new List<AdditionalFee>();

        
        var damageFee = additionalFees
            .Where(f => f.FeeType == AdditionalFeeTypeEnum.DAMAGE.ToString())
            .Sum(f => f.Amount);

        var cleaningFee = additionalFees
            .Where(f => f.FeeType == AdditionalFeeTypeEnum.CLEANING.ToString())
            .Sum(f => f.Amount);

        var lateReturnFee = additionalFees
            .Where(f => f.FeeType == AdditionalFeeTypeEnum.LATE_RETURN.ToString())
            .Sum(f => f.Amount);

        var crossBranchFee = additionalFees
            .Where(f => f.FeeType == AdditionalFeeTypeEnum.CROSS_BRANCH.ToString())
            .Sum(f => f.Amount);

        var excessKmFee = additionalFees
            .Where(f => f.FeeType == AdditionalFeeTypeEnum.EXCCESS_KM.ToString())
            .Sum(f => f.Amount);

        var totalAdditionalFees = damageFee + cleaningFee + lateReturnFee + crossBranchFee + excessKmFee;
        var totalReturnAmount = totalChargingFee + totalAdditionalFees;
        var refundAmount = booking.DepositAmount - totalReturnAmount;
        var totalAmount = booking.TotalAmount + totalReturnAmount;

        return new SettlementSummary
        {
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

           
            var booking = await _unitOfWork.GetBookingRepository()
                .GetBookingForSettlementAsync(request.BookingId);

            if (booking == null)
            {
                return ResultResponse<UpdateReturnReceiptResponse>.Failure("Booking not found");
            }

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

            
            if (request.EndOdometerKm.HasValue)
            {
                if (request.EndOdometerKm.Value < rentalReceipt.StartOdometerKm)
                {
                    return ResultResponse<UpdateReturnReceiptResponse>.Failure(
                        $"End odometer ({request.EndOdometerKm.Value} km) cannot be less than start odometer ({rentalReceipt.StartOdometerKm} km)");
                }

                rentalReceipt.EndOdometerKm = request.EndOdometerKm.Value;
                updateSummary.OdometerUpdated = true;
            }

            
            if (request.EndBatteryPercentage.HasValue)
            {
                if (request.EndBatteryPercentage.Value < 0 || request.EndBatteryPercentage.Value > 100)
                {
                    return ResultResponse<UpdateReturnReceiptResponse>.Failure(
                        "Battery percentage must be between 0 and 100");
                }

                rentalReceipt.EndBatteryPercentage = request.EndBatteryPercentage.Value;
                updateSummary.BatteryUpdated = true;
            }

            
            if (!string.IsNullOrEmpty(request.Notes))
            {
                rentalReceipt.Notes = request.Notes;
                updateSummary.NotesUpdated = true;
            }

            
            if (request.NewReturnImages != null && request.NewReturnImages.Any())
            {
                
                var oldReturnImages = await _unitOfWork.GetMediaRepository()
                    .GetMediaByDocNoAndTypeAsync(
                        rentalReceipt.Id,
                        MediaEntityTypeEnum.RentalReceiptReturnImage.ToString()
                    );

                foreach (var oldImage in oldReturnImages)
                {
                    await _cloudinaryService.DeleteImageFileByUrlAsync(
                        oldImage.FileUrl,
                        "RentalReceipt/Return"
                    );
                    _unitOfWork.GetMediaRepository().Delete(oldImage);
                }

                // Upload ảnh mới
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

            
            VehicleVerificationResult? newVerificationResult = null;
            DamageDetectionResult? newDamageResult = null;

            if (request.RerunAIAnalysis)
            {
                var currentReturnImages = await _unitOfWork.GetMediaRepository()
                    .GetMediaByDocNoAndTypeAsync(
                        rentalReceipt.Id,
                        MediaEntityTypeEnum.RentalReceiptReturnImage.ToString()
                    );

                var returnUrls = currentReturnImages.Select(m => m.FileUrl).ToList();

                var handoverImages = await _unitOfWork.GetMediaRepository()
                    .GetMediaByDocNoAndTypeAsync(
                        rentalReceipt.Id,
                        MediaEntityTypeEnum.RentalReceiptHandoverImage.ToString()
                    );

                var handoverUrls = handoverImages.Select(m => m.FileUrl).ToList();

                if (returnUrls.Any() && handoverUrls.Any())
                {
                    newVerificationResult = await _geminiAIService.VerifyVehicleAsync(
                        handoverUrls,
                        returnUrls,
                        booking.Vehicle.LicensePlate
                    );

                    newDamageResult = await _geminiAIService.DetectDamagesAsync(
                        handoverUrls,
                        returnUrls
                    );

                    updateSummary.AIAnalysisRerun = true;
                }
            }

            
            if (request.NewChecklistImage != null)
            {
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



            
            _unitOfWork.GetRentalReceiptRepository().Update(rentalReceipt);
            await _unitOfWork.SaveChangesAsync();

            
            await _unitOfWork.GetBookingRepository()
                .Query()
                .Where(b => b.Id == request.BookingId)
                .Include(b => b.ChargingRecords)
                .Include(b => b.AdditionalFees)
                .LoadAsync();

            var newSettlement = await CalculateSettlementAsync(booking);

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
                "Return receipt updated successfully. To modify additional fees, use AdditionalFee APIs.",
                response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<UpdateReturnReceiptResponse>.Failure(
                $"An error occurred: {ex.Message}");
        }
    }


    public async Task<ResultResponse<string>> DeleteReturnReceiptAsync(Guid bookingId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            
            var booking = await _unitOfWork.GetBookingRepository()
                .GetBookingForSettlementAsync(bookingId);

            if (booking == null)
            {
                return ResultResponse<string>.Failure("Booking not found");
            }

            
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

            
            var additionalFees = await _unitOfWork.GetAdditionalFeeRepository()
                .GetAdditionalFeesByBookingIdAsync(bookingId);

            foreach (var fee in additionalFees)
            {
                _unitOfWork.GetAdditionalFeeRepository().Delete(fee);
            }

            
            rentalReceipt.EndOdometerKm = 0;
            rentalReceipt.EndBatteryPercentage = 0;
            rentalReceipt.Notes = null;
            rentalReceipt.RenterConfirmedAt = null;

            _unitOfWork.GetRentalReceiptRepository().Update(rentalReceipt);

            
            booking.TotalAdditionalFee = 0;
            booking.TotalAmount = booking.TotalRentalFee + booking.TotalChargingFee;
            booking.RefundAmount = booking.DepositAmount - booking.TotalAmount;
            booking.LateReturnFee = 0;
            booking.CleaningFee = 0;
            booking.CrossBranchFee = 0;
            booking.ExcessKmFee = 0;
            booking.ActualReturnDatetime = null;

            
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