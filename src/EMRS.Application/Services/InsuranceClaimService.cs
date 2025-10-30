using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.InsuranceClaimDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services
{
    public class InsuranceClaimService : IInsuranceClaimService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICloudinaryService _cloudinaryService;

        public InsuranceClaimService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<ResultResponse<InsuranceClaimResponse>> CreateInsuranceClaim(
            CreateInsuranceClaimRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var renterId = Guid.Parse(_currentUserService.UserId!);

                var booking = await _unitOfWork.GetBookingRepository()
                    .Query()
                    .Include(b => b.InsurancePackage)
                    .Where(b => b.Id == request.BookingId)
                    .SingleOrDefaultAsync();

                if (booking == null)
                    return ResultResponse<InsuranceClaimResponse>.NotFound("Booking not found");

                if (booking.RenterId != renterId)
                    return ResultResponse<InsuranceClaimResponse>.Forbidden("This booking does not belong to you");

                // Check if booking has insurance package
                if (booking.InsurancePackageId == null || booking.InsurancePackage == null)
                {
                    return ResultResponse<InsuranceClaimResponse>.Failure(
                        "Cannot create insurance claim. This booking does not have an insurance package");
                }

                if (booking.BookingStatus != BookingStatusEnum.Renting.ToString() &&
                    booking.BookingStatus != BookingStatusEnum.Returned.ToString())
                {
                    return ResultResponse<InsuranceClaimResponse>.Failure(
                        "Insurance claim can only be reported for active or recently returned rentals");
                }

                var insuranceClaim = new InsuranceClaim
                {
                    BookingId = request.BookingId,
                    RenterId = renterId,
                    IncidentDate = request.IncidentDate,
                    IncidentLocation = request.IncidentLocation,
                    Description = request.Description,
                    Status = InsuranceClaimStatusEnum.Reported.ToString(),
                    Severity = string.Empty,
                    VehicleDamageCost = 0,
                    PersonInjuryCost = 0,
                    ThirdPartyCost = 0,
                    TotalCost = 0,
                    InsuranceCoverageAmount = 0,
                    RenterLiabilityAmount = 0
                };

                await _unitOfWork.GetInsuranceClaimRepository().AddAsync(insuranceClaim);
                await _unitOfWork.SaveChangesAsync();

                if (request.IncidentImageFiles != null && request.IncidentImageFiles.Any())
                {
                    var uploadTasks = new List<Task<string?>>();
                    var mediaList = new List<Media>();

                    for (int i = 0; i < request.IncidentImageFiles.Count; i++)
                    {
                        var file = request.IncidentImageFiles[i];
                        if (file != null && file.Length > 0)
                        {
                            var fileName = $"incident_{insuranceClaim.Id}_{i}_{DateTime.UtcNow:yyyyMMddHHmmss}";
                            var folderName = $"insurance_claims/{insuranceClaim.Id}";

                            var uploadTask = _cloudinaryService.UploadImageFileAsync(file, fileName, folderName);
                            uploadTasks.Add(uploadTask);
                        }
                    }

                    var uploadResults = await Task.WhenAll(uploadTasks);

                    foreach (var imageUrl in uploadResults)
                    {
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            var media = new Media
                            {
                                MediaType = MediaEntityTypeEnum.InsuranceClaim.ToString(),
                                FileUrl = imageUrl,
                                DocNo = insuranceClaim.Id,
                                EntityType = "InsuranceClaim"
                            };
                            mediaList.Add(media);
                        }
                    }

                    foreach (var media in mediaList)
                    {
                        await _unitOfWork.GetMediaRepository().AddAsync(media);
                    }

                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitAsync();

                var response = _mapper.Map<InsuranceClaimResponse>(insuranceClaim);

                return ResultResponse<InsuranceClaimResponse>.SuccessResult(
                    "Insurance claim reported successfully. Manager will review your report soon.",
                    response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<InsuranceClaimResponse>.Failure(
                    $"An error occurred while creating insurance claim: {ex.Message}");
            }
        }

        public async Task<ResultResponse<List<InsuranceClaimResponse>>> GetMyInsuranceClaims()
        {
            try
            {
                var renterId = Guid.Parse(_currentUserService.UserId!);

                var insuranceClaims = await _unitOfWork.GetInsuranceClaimRepository()
                    .GetInsuranceClaimsByRenterIdAsync(renterId);

                var response = insuranceClaims.Select(ic => new InsuranceClaimResponse
                {
                    Id = ic.Id,
                    IncidentDate = ic.IncidentDate,
                    IncidentLocation = ic.IncidentLocation,
                    Description = ic.Description,
                    Status = ic.Status,
                    ModelName = ic.Booking.VehicleModel.ModelName,
                    LicensePlate = ic.Booking.Vehicle?.LicensePlate ?? "Not Assigned",
                    PackageName = ic.Booking.InsurancePackage!.PackageName,
                    PackageFee = ic.Booking.InsurancePackage.PackageFee,
                    CoveragePersonLimit = ic.Booking.InsurancePackage.CoveragePersonLimit,
                    CoveragePropertyLimit = ic.Booking.InsurancePackage.CoveragePropertyLimit,
                    CoverageVehiclePercentage = ic.Booking.InsurancePackage.CoverageVehiclePercentage,
                    CoverageTheft = ic.Booking.InsurancePackage.CoverageTheft,
                    DeductibleAmount = ic.Booking.InsurancePackage.DeductibleAmount,
                    VehicleDamageCost = ic.Status == InsuranceClaimStatusEnum.Completed.ToString() ? ic.VehicleDamageCost : null,
                    PersonInjuryCost = ic.Status == InsuranceClaimStatusEnum.Completed.ToString() ? ic.PersonInjuryCost : null,
                    ThirdPartyCost = ic.Status == InsuranceClaimStatusEnum.Completed.ToString() ? ic.ThirdPartyCost : null,
                    TotalCost = ic.Status == InsuranceClaimStatusEnum.Completed.ToString() ? ic.TotalCost : null,
                    InsuranceCoverageAmount = ic.Status == InsuranceClaimStatusEnum.Completed.ToString() ? ic.InsuranceCoverageAmount : null,
                    RenterLiabilityAmount = ic.Status == InsuranceClaimStatusEnum.Completed.ToString() ? ic.RenterLiabilityAmount : null,
                    BookingId = ic.BookingId,
                    RenterId = ic.RenterId,
                    CreatedAt = ic.CreatedAt
                }).ToList();

                return ResultResponse<List<InsuranceClaimResponse>>.SuccessResult(
                    "Insurance claims retrieved successfully",
                    response);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<InsuranceClaimResponse>>.Failure(
                    $"An error occurred: {ex.Message}");
            }
        }

        public async Task<ResultResponse<InsuranceClaimDetailResponse>> GetInsuranceClaimDetail(Guid id)
        {
            try
            {
                var userId = Guid.Parse(_currentUserService.UserId!);
                var roles = _currentUserService.Roles;

                var insuranceClaim = await _unitOfWork.GetInsuranceClaimRepository()
                    .GetInsuranceClaimWithDetailsAsync(id);

                if (insuranceClaim == null)
                    return ResultResponse<InsuranceClaimDetailResponse>.NotFound("Insurance claim not found");

                if (!roles.Contains("MANAGER") && !roles.Contains("ADMIN") && insuranceClaim.RenterId != userId)
                {
                    return ResultResponse<InsuranceClaimDetailResponse>.Forbidden(
                        "You do not have permission to view this insurance claim");
                }

                var incidentImages = await _unitOfWork.GetMediaRepository()
                    .Query()
                    .Where(m => m.DocNo == id && m.EntityType == "InsuranceClaim")
                    .Select(m => m.FileUrl)
                    .ToListAsync();

                var response = _mapper.Map<InsuranceClaimDetailResponse>(insuranceClaim);
                response.IncidentImages = incidentImages;

                return ResultResponse<InsuranceClaimDetailResponse>.SuccessResult(
                    "Insurance claim retrieved successfully",
                    response);
            }
            catch (Exception ex)
            {
                return ResultResponse<InsuranceClaimDetailResponse>.Failure(
                    $"An error occurred: {ex.Message}");
            }
        }

        // NEW: Get all insurance claims for manager's branch
        public async Task<ResultResponse<List<InsuranceClaimListForManagerResponse>>> GetBranchInsuranceClaims()
        {
            try
            {
                var userId = Guid.Parse(_currentUserService.UserId!);
                // Get staff record to find branch
                var staff = await _unitOfWork.GetStaffRepository()
                    .Query()
                    .Where(s => s.Id == userId)
                    .SingleOrDefaultAsync();

                if (staff == null || staff.BranchId == null)
                    return ResultResponse<List<InsuranceClaimListForManagerResponse>>.NotFound("Staff or branch not found");

                var insuranceClaims = await _unitOfWork.GetInsuranceClaimRepository()
                    .GetInsuranceClaimsByBranchIdAsync(staff.BranchId.Value);

                var response = insuranceClaims.Select(ic => new InsuranceClaimListForManagerResponse
                {
                    Id = ic.Id,
                    Status = ic.Status,
                    IncidentDate = ic.IncidentDate,
                    IncidentLocation = ic.IncidentLocation,
                    RenterName = ic.Renter.Account.Fullname ?? "Unknown",
                    RenterPhone = ic.Renter.phone,
                    VehicleModelName = ic.Booking.Vehicle!.VehicleModel.ModelName,
                    LicensePlate = ic.Booking.Vehicle.LicensePlate,
                    BookingId = ic.BookingId,
                    HandoverBranchName = ic.Booking.HandoverBranch?.BranchName ?? "Unknown",
                    CreatedAt = ic.CreatedAt
                }).ToList();

                return ResultResponse<List<InsuranceClaimListForManagerResponse>>.SuccessResult(
                    "Insurance claims retrieved successfully",
                    response);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<InsuranceClaimListForManagerResponse>>.Failure(
                    $"An error occurred: {ex.Message}");
            }
        }

        // NEW: Get insurance claim detail for manager
        public async Task<ResultResponse<InsuranceClaimForManagerResponse>> GetInsuranceClaimForManager(Guid id)
        {
            try
            {
                var userId = Guid.Parse(_currentUserService.UserId!);

                // Get staff record to verify branch access
                var staff = await _unitOfWork.GetStaffRepository()
                    .Query()
                    .Where(s => s.Id == userId)
                    .SingleOrDefaultAsync();

                if (staff == null || staff.BranchId == null)
                    return ResultResponse<InsuranceClaimForManagerResponse>.NotFound("Staff or branch not found");

                var insuranceClaim = await _unitOfWork.GetInsuranceClaimRepository()
                    .GetInsuranceClaimForManagerAsync(id);

                if (insuranceClaim == null)
                    return ResultResponse<InsuranceClaimForManagerResponse>.NotFound("Insurance claim not found");

                // Verify this claim belongs to manager's branch
                if (insuranceClaim.Booking.Vehicle?.BranchId != staff.BranchId)
                {
                    return ResultResponse<InsuranceClaimForManagerResponse>.Forbidden(
                        "This insurance claim does not belong to your branch");
                }

                // Get incident images
                var incidentImages = await _unitOfWork.GetMediaRepository()
                    .Query()
                    .Where(m => m.DocNo == id && m.EntityType == "InsuranceClaim")
                    .Select(m => m.FileUrl)
                    .ToListAsync();

                var response = new InsuranceClaimForManagerResponse
                {
                    Id = insuranceClaim.Id,
                    Status = insuranceClaim.Status,
                    IncidentDate = insuranceClaim.IncidentDate,
                    IncidentLocation = insuranceClaim.IncidentLocation,
                    Description = insuranceClaim.Description,
                    RenterName = insuranceClaim.Renter.Account.Fullname ?? "Unknown",
                    RenterPhone = insuranceClaim.Renter.phone,
                    RenterEmail = insuranceClaim.Renter.Email,
                    Address = insuranceClaim.Renter.Address,
                    VehicleModelName = insuranceClaim.Booking.Vehicle!.VehicleModel.ModelName,
                    LicensePlate = insuranceClaim.Booking.Vehicle.LicensePlate,
                    VehicleDescription = insuranceClaim.Booking.Vehicle.Description,
                    BookingId = insuranceClaim.BookingId,
                    HandoverBranchName = insuranceClaim.Booking.HandoverBranch?.BranchName ?? "Unknown",
                    HandoverBranchAddress = insuranceClaim.Booking.HandoverBranch?.Address ?? "Unknown",
                    BookingStartDate = insuranceClaim.Booking.StartDatetime,
                    BookingEndDate = insuranceClaim.Booking.EndDatetime,
                    PackageName = insuranceClaim.Booking.InsurancePackage!.PackageName,
                    PackageFee = insuranceClaim.Booking.InsurancePackage.PackageFee,
                    CoveragePersonLimit = insuranceClaim.Booking.InsurancePackage.CoveragePersonLimit,
                    CoveragePropertyLimit = insuranceClaim.Booking.InsurancePackage.CoveragePropertyLimit,
                    CoverageVehiclePercentage = insuranceClaim.Booking.InsurancePackage.CoverageVehiclePercentage,
                    CoverageTheft = insuranceClaim.Booking.InsurancePackage.CoverageTheft,
                    DeductibleAmount = insuranceClaim.Booking.InsurancePackage.DeductibleAmount,
                    InsuranceDescription = insuranceClaim.Booking.InsurancePackage.Description,
                    IncidentImages = incidentImages,
                    CreatedAt = insuranceClaim.CreatedAt
                };

                return ResultResponse<InsuranceClaimForManagerResponse>.SuccessResult(
                    "Insurance claim retrieved successfully",
                    response);
            }
            catch (Exception ex)
            {
                return ResultResponse<InsuranceClaimForManagerResponse>.Failure(
                    $"An error occurred: {ex.Message}");
            }
        }


        // API 5: Update Insurance Claim
        public async Task<ResultResponse<InsuranceClaimForManagerResponse>> UpdateInsuranceClaim(
            Guid id,
            UpdateInsuranceClaimRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var userId = Guid.Parse(_currentUserService.UserId!);

                // Get staff record to verify branch access
                var staff = await _unitOfWork.GetStaffRepository()
                    .Query()
                    .Where(s => s.AccountId == userId)
                    .SingleOrDefaultAsync();

                if (staff == null || staff.BranchId == null)
                    return ResultResponse<InsuranceClaimForManagerResponse>.NotFound("Staff or branch not found");

                // Get insurance claim with full details
                var insuranceClaim = await _unitOfWork.GetInsuranceClaimRepository()
                    .GetInsuranceClaimForManagerAsync(id);

                if (insuranceClaim == null)
                    return ResultResponse<InsuranceClaimForManagerResponse>.NotFound("Insurance claim not found");

                // Verify this claim belongs to manager's branch
                if (insuranceClaim.Booking.Vehicle?.BranchId != staff.BranchId)
                {
                    return ResultResponse<InsuranceClaimForManagerResponse>.Forbidden(
                        "This insurance claim does not belong to your branch");
                }

                // CRITICAL: Check if claim is locked (Processing or Completed)
                if (insuranceClaim.Status == InsuranceClaimStatusEnum.Processing.ToString() ||
                    insuranceClaim.Status == InsuranceClaimStatusEnum.Completed.ToString())
                {
                    return ResultResponse<InsuranceClaimForManagerResponse>.Failure(
                        "Cannot update claim that is already processing or completed");
                }

                // Update fields if provided
                if (request.IncidentDate.HasValue)
                    insuranceClaim.IncidentDate = request.IncidentDate;

                if (!string.IsNullOrEmpty(request.IncidentLocation))
                    insuranceClaim.IncidentLocation = request.IncidentLocation;

                if (!string.IsNullOrEmpty(request.Description))
                    insuranceClaim.Description = request.Description;

                if (!string.IsNullOrEmpty(request.Severity))
                {
                    // Validate Severity enum
                    if (!Enum.TryParse<InsuranceClaimSeverityEnum>(request.Severity, out _))
                    {
                        return ResultResponse<InsuranceClaimForManagerResponse>.Failure(
                            "Invalid severity value. Must be: Minor, Moderate, Severe, or Critical");
                    }
                    insuranceClaim.Severity = request.Severity;
                }

                if (!string.IsNullOrEmpty(request.Notes))
                    insuranceClaim.Notes = request.Notes;

                // Handle Status changes
                if (!string.IsNullOrEmpty(request.Status))
                {
                    // Validate status enum
                    if (!Enum.TryParse<InsuranceClaimStatusEnum>(request.Status, out var newStatus))
                    {
                        return ResultResponse<InsuranceClaimForManagerResponse>.Failure(
                            "Invalid status value");
                    }

                    // Special handling for Rejection
                    if (newStatus == InsuranceClaimStatusEnum.Rejected)
                    {
                        if (string.IsNullOrEmpty(request.RejectionReason))
                        {
                            return ResultResponse<InsuranceClaimForManagerResponse>.Failure(
                                "Rejection reason is required when rejecting a claim");
                        }
                        insuranceClaim.RejectionReason = request.RejectionReason;
                        insuranceClaim.ReviewedDate = DateTime.UtcNow;
                    }

                    // Special handling for Processing (lock the claim)
                    if (newStatus == InsuranceClaimStatusEnum.Processing)
                    {
                        insuranceClaim.ReviewedDate = DateTime.UtcNow;
                    }

                    insuranceClaim.Status = request.Status;
                }
                else if (!string.IsNullOrEmpty(request.RejectionReason))
                {
                    // If rejection reason provided without status change, save it anyway
                    insuranceClaim.RejectionReason = request.RejectionReason;
                }

                // Upload additional images if provided
                if (request.AdditionalImageFiles != null && request.AdditionalImageFiles.Any())
                {
                    var uploadTasks = new List<Task<string?>>();
                    var mediaList = new List<Media>();

                    // Get current image count for naming
                    var currentImageCount = await _unitOfWork.GetMediaRepository()
                        .Query()
                        .Where(m => m.DocNo == id && m.EntityType == "InsuranceClaim")
                        .CountAsync();

                    for (int i = 0; i < request.AdditionalImageFiles.Count; i++)
                    {
                        var file = request.AdditionalImageFiles[i];
                        if (file != null && file.Length > 0)
                        {
                            var fileName = $"incident_{id}_{currentImageCount + i}_{DateTime.UtcNow:yyyyMMddHHmmss}";
                            var folderName = $"insurance_claims/{id}";

                            var uploadTask = _cloudinaryService.UploadImageFileAsync(file, fileName, folderName);
                            uploadTasks.Add(uploadTask);
                        }
                    }

                    var uploadResults = await Task.WhenAll(uploadTasks);

                    foreach (var imageUrl in uploadResults)
                    {
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            var media = new Media
                            {
                                MediaType = MediaEntityTypeEnum.InsuranceClaim.ToString(),
                                FileUrl = imageUrl,
                                DocNo = id,
                                EntityType = "InsuranceClaim"
                            };
                            mediaList.Add(media);
                        }
                    }

                    foreach (var media in mediaList)
                    {
                        await _unitOfWork.GetMediaRepository().AddAsync(media);
                    }
                }

                _unitOfWork.GetInsuranceClaimRepository().Update(insuranceClaim);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                // Get updated claim with all images
                var incidentImages = await _unitOfWork.GetMediaRepository()
                    .Query()
                    .Where(m => m.DocNo == id && m.EntityType == "InsuranceClaim")
                    .Select(m => m.FileUrl)
                    .ToListAsync();

                var response = new InsuranceClaimForManagerResponse
                {
                    Id = insuranceClaim.Id,
                    Status = insuranceClaim.Status,
                    IncidentDate = insuranceClaim.IncidentDate,
                    IncidentLocation = insuranceClaim.IncidentLocation,
                    Description = insuranceClaim.Description,
                    RenterName = insuranceClaim.Renter.Account.Fullname ?? "Unknown",
                    RenterPhone = insuranceClaim.Renter.phone,
                    RenterEmail = insuranceClaim.Renter.Email,
                    Address = insuranceClaim.Renter.Address,
                    VehicleModelName = insuranceClaim.Booking.Vehicle!.VehicleModel.ModelName,
                    LicensePlate = insuranceClaim.Booking.Vehicle.LicensePlate,
                    VehicleDescription = insuranceClaim.Booking.Vehicle.Description,
                    BookingId = insuranceClaim.BookingId,
                    HandoverBranchName = insuranceClaim.Booking.HandoverBranch?.BranchName ?? "Unknown",
                    HandoverBranchAddress = insuranceClaim.Booking.HandoverBranch?.Address ?? "Unknown",
                    BookingStartDate = insuranceClaim.Booking.StartDatetime,
                    BookingEndDate = insuranceClaim.Booking.EndDatetime,
                    PackageName = insuranceClaim.Booking.InsurancePackage!.PackageName,
                    PackageFee = insuranceClaim.Booking.InsurancePackage.PackageFee,
                    CoveragePersonLimit = insuranceClaim.Booking.InsurancePackage.CoveragePersonLimit,
                    CoveragePropertyLimit = insuranceClaim.Booking.InsurancePackage.CoveragePropertyLimit,
                    CoverageVehiclePercentage = insuranceClaim.Booking.InsurancePackage.CoverageVehiclePercentage,
                    CoverageTheft = insuranceClaim.Booking.InsurancePackage.CoverageTheft,
                    DeductibleAmount = insuranceClaim.Booking.InsurancePackage.DeductibleAmount,
                    InsuranceDescription = insuranceClaim.Booking.InsurancePackage.Description,
                    IncidentImages = incidentImages,
                    CreatedAt = insuranceClaim.CreatedAt
                };

                return ResultResponse<InsuranceClaimForManagerResponse>.SuccessResult(
                    "Insurance claim updated successfully",
                    response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<InsuranceClaimForManagerResponse>.Failure(
                    $"An error occurred while updating insurance claim: {ex.Message}");
            }
        }

        // API 6: Complete Insurance Settlement
        public async Task<ResultResponse<InsuranceClaimForManagerResponse>> CompleteInsuranceSettlement(
            Guid id,
            InsuranceSettlementRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var userId = Guid.Parse(_currentUserService.UserId!);

                // Get staff record
                var staff = await _unitOfWork.GetStaffRepository()
                    .Query()
                    .Where(s => s.AccountId == userId)
                    .SingleOrDefaultAsync();

                if (staff == null || staff.BranchId == null)
                    return ResultResponse<InsuranceClaimForManagerResponse>.NotFound("Staff or branch not found");

                // Get insurance claim with full details
                var insuranceClaim = await _unitOfWork.GetInsuranceClaimRepository()
                    .Query()
                    .Include(ic => ic.Booking)
                        .ThenInclude(b => b.Vehicle)
                            .ThenInclude(v => v!.VehicleModel)
                    .Include(ic => ic.Booking)
                        .ThenInclude(b => b.InsurancePackage)
                    .Include(ic => ic.Booking)
                        .ThenInclude(b => b.HandoverBranch)
                    .Include(ic => ic.Renter)
                        .ThenInclude(r => r.Account)
                    .Include(ic => ic.Renter)
                        .ThenInclude(r => r.Wallet)
                    .Where(ic => ic.Id == id)
                    .SingleOrDefaultAsync();

                if (insuranceClaim == null)
                    return ResultResponse<InsuranceClaimForManagerResponse>.NotFound("Insurance claim not found");

                // Verify branch access
                if (insuranceClaim.Booking.Vehicle?.BranchId != staff.BranchId)
                {
                    return ResultResponse<InsuranceClaimForManagerResponse>.Forbidden(
                        "This insurance claim does not belong to your branch");
                }

                // CRITICAL: Must be in Processing status
                if (insuranceClaim.Status != InsuranceClaimStatusEnum.Processing.ToString())
                {
                    return ResultResponse<InsuranceClaimForManagerResponse>.Failure(
                        "Insurance claim must be in 'Processing' status to complete settlement");
                }

                // Calculate total cost
                var totalCost = request.VehicleDamageCost + request.PersonInjuryCost + request.ThirdPartyCost;

                // Calculate renter liability (applying deductible logic)
                var deductibleAmount = insuranceClaim.Booking.InsurancePackage!.DeductibleAmount;
                var renterLiabilityAmount = totalCost - request.InsuranceCoverageAmount;

                // Validate: Insurance coverage cannot exceed total cost
                if (request.InsuranceCoverageAmount > totalCost)
                {
                    return ResultResponse<InsuranceClaimForManagerResponse>.Failure(
                        "Insurance coverage amount cannot exceed total cost");
                }

                // Update insurance claim financial details
                insuranceClaim.VehicleDamageCost = request.VehicleDamageCost;
                insuranceClaim.PersonInjuryCost = request.PersonInjuryCost;
                insuranceClaim.ThirdPartyCost = request.ThirdPartyCost;
                insuranceClaim.TotalCost = totalCost;
                insuranceClaim.InsuranceCoverageAmount = request.InsuranceCoverageAmount;
                insuranceClaim.RenterLiabilityAmount = renterLiabilityAmount;
                insuranceClaim.Status = InsuranceClaimStatusEnum.Completed.ToString();
                insuranceClaim.CompletedAt = DateTime.UtcNow;

                // Upload insurance PDF if provided
                if (request.InsuranceClaimPdfFile != null && request.InsuranceClaimPdfFile.Length > 0)
                {
                    var fileName = $"insurance_settlement_{id}_{DateTime.UtcNow:yyyyMMddHHmmss}";
                    var folderName = $"insurance_claims/{id}";

                    var pdfUrl = await _cloudinaryService.UploadDocumentFileAsync(
                        request.InsuranceClaimPdfFile,
                        fileName,
                        folderName);

                    if (!string.IsNullOrEmpty(pdfUrl))
                    {
                        insuranceClaim.InsuranceClaimPdfUrl = pdfUrl;
                    }
                }

                // PAYMENT PROCESSING
                var booking = insuranceClaim.Booking;
                var wallet = insuranceClaim.Renter.Wallet;

                if (wallet == null)
                {
                    return ResultResponse<InsuranceClaimForManagerResponse>.Failure(
                        "Renter wallet not found");
                }

                var remainingLiability = renterLiabilityAmount;

                // Step 1: Deduct from deposit
                var depositUsed = Math.Min(booking.DepositAmount, remainingLiability);
                remainingLiability -= depositUsed;

                // Create transaction for deposit usage
                if (depositUsed > 0)
                {
                    var depositTransaction = new Transaction
                    {
                        TransactionType = TransactionTypeEnum.InsuranceClaimPayment.ToString(),
                        Amount = depositUsed,
                        DocNo = id,
                        Status = "Completed"
                    };
                    await _unitOfWork.GetTransactionRepository().AddAsync(depositTransaction);
                }

                // Step 2: Deduct remaining from wallet (if any)
                if (remainingLiability > 0)
                {
                    // Note: Tạm thời bỏ qua việc check insufficient balance theo yêu cầu
                    wallet.Balance -= remainingLiability;

                    var walletTransaction = new Transaction
                    {
                        TransactionType = TransactionTypeEnum.InsuranceClaimPayment.ToString(),
                        Amount = remainingLiability,
                        DocNo = id,
                        Status = "Completed"
                    };
                    await _unitOfWork.GetTransactionRepository().AddAsync(walletTransaction);
                }

                // Step 3: Refund excess deposit
                var refundAmount = booking.DepositAmount - depositUsed;
                if (refundAmount > 0)
                {
                    wallet.Balance += refundAmount;
                    booking.RefundAmount += refundAmount; // Accumulate refund

                    var refundTransaction = new Transaction
                    {
                        TransactionType = TransactionTypeEnum.InsuranceClaimRefund.ToString(),
                        Amount = refundAmount,
                        DocNo = id,
                        Status = "Completed"
                    };
                    await _unitOfWork.GetTransactionRepository().AddAsync(refundTransaction);
                }

                // Update entities
                _unitOfWork.GetInsuranceClaimRepository().Update(insuranceClaim);
                _unitOfWork.GetBookingRepository().Update(booking);
                _unitOfWork.GetWalletRepository().Update(wallet);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                // Get all images for response
                var incidentImages = await _unitOfWork.GetMediaRepository()
                    .Query()
                    .Where(m => m.DocNo == id && m.EntityType == "InsuranceClaim")
                    .Select(m => m.FileUrl)
                    .ToListAsync();

                var response = new InsuranceClaimForManagerResponse
                {
                    Id = insuranceClaim.Id,
                    Status = insuranceClaim.Status,
                    IncidentDate = insuranceClaim.IncidentDate,
                    IncidentLocation = insuranceClaim.IncidentLocation,
                    Description = insuranceClaim.Description,
                    RenterName = insuranceClaim.Renter.Account.Fullname ?? "Unknown",
                    RenterPhone = insuranceClaim.Renter.phone,
                    RenterEmail = insuranceClaim.Renter.Email,
                    Address = insuranceClaim.Renter.Address,
                    VehicleModelName = insuranceClaim.Booking.Vehicle!.VehicleModel.ModelName,
                    LicensePlate = insuranceClaim.Booking.Vehicle.LicensePlate,
                    VehicleDescription = insuranceClaim.Booking.Vehicle.Description,
                    BookingId = insuranceClaim.BookingId,
                    HandoverBranchName = insuranceClaim.Booking.HandoverBranch?.BranchName ?? "Unknown",
                    HandoverBranchAddress = insuranceClaim.Booking.HandoverBranch?.Address ?? "Unknown",
                    BookingStartDate = insuranceClaim.Booking.StartDatetime,
                    BookingEndDate = insuranceClaim.Booking.EndDatetime,
                    PackageName = insuranceClaim.Booking.InsurancePackage!.PackageName,
                    PackageFee = insuranceClaim.Booking.InsurancePackage.PackageFee,
                    CoveragePersonLimit = insuranceClaim.Booking.InsurancePackage.CoveragePersonLimit,
                    CoveragePropertyLimit = insuranceClaim.Booking.InsurancePackage.CoveragePropertyLimit,
                    CoverageVehiclePercentage = insuranceClaim.Booking.InsurancePackage.CoverageVehiclePercentage,
                    CoverageTheft = insuranceClaim.Booking.InsurancePackage.CoverageTheft,
                    DeductibleAmount = insuranceClaim.Booking.InsurancePackage.DeductibleAmount,
                    InsuranceDescription = insuranceClaim.Booking.InsurancePackage.Description,
                    IncidentImages = incidentImages,
                    CreatedAt = insuranceClaim.CreatedAt
                };

                return ResultResponse<InsuranceClaimForManagerResponse>.SuccessResult(
                    $"Insurance settlement completed. Renter liability: {renterLiabilityAmount:N0} VND. Refund: {refundAmount:N0} VND",
                    response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<InsuranceClaimForManagerResponse>.Failure(
                    $"An error occurred while completing settlement: {ex.Message}");
            }
        }
    }
}
