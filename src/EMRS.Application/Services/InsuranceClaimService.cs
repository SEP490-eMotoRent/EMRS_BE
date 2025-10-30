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
                    .Where(s => s.AccountId == userId)
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
                    .Where(s => s.AccountId == userId)
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
    }
}
