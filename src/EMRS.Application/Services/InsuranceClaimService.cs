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

                // 1. Get current user (Renter)
                var renterId = Guid.Parse(_currentUserService.UserId!);

                // 2. Validate booking exists and belongs to renter
                var booking = await _unitOfWork.GetBookingRepository().FindByIdAsync(request.BookingId);
                if (booking == null)
                    return ResultResponse<InsuranceClaimResponse>.NotFound("Booking not found");

                if (booking.RenterId != renterId)
                    return ResultResponse<InsuranceClaimResponse>.Forbidden("This booking does not belong to you");

                // 3. Check booking status (must be "Renting" or "Returned")
                if (booking.BookingStatus != BookingStatusEnum.Renting.ToString() &&
                    booking.BookingStatus != BookingStatusEnum.Returned.ToString())
                {
                    return ResultResponse<InsuranceClaimResponse>.Failure(
                        "Insurance claim can only be reported for active or recently returned rentals");
                }

                // 4. Create InsuranceClaim entity
                var insuranceClaim = new InsuranceClaim
                {
                    BookingId = request.BookingId,
                    RenterId = renterId,
                    IncidentDate = request.IncidentDate,
                    IncidentLocation = request.IncidentLocation,
                    Description = request.Description,
                    Status = InsuranceClaimStatusEnum.Reported.ToString(),
                    Severity = string.Empty, // Not required as per your decision
                    VehicleDamageCost = 0,
                    PersonInjuryCost = 0,
                    ThirdPartyCost = 0,
                    TotalCost = 0,
                    InsuranceCoverageAmount = 0,
                    RenterLiabilityAmount = 0
                };

                await _unitOfWork.GetInsuranceClaimRepository().AddAsync(insuranceClaim);
                await _unitOfWork.SaveChangesAsync();

                // 5. Upload incident images to Cloudinary and save to Media table
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

                    // Save all media records
                    foreach (var media in mediaList)
                    {
                        await _unitOfWork.GetMediaRepository().AddAsync(media);
                    }

                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitAsync();

                // 6. Map to response DTO
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

        public async Task<ResultResponse<InsuranceClaimDetailResponse>> GetInsuranceClaimDetail(Guid id)
        {
            try
            {
                // Get current user
                var userId = Guid.Parse(_currentUserService.UserId!);
                var roles = _currentUserService.Roles;

                // Get insurance claim with details
                var insuranceClaim = await _unitOfWork.GetInsuranceClaimRepository()
                    .GetInsuranceClaimWithDetailsAsync(id);

                if (insuranceClaim == null)
                    return ResultResponse<InsuranceClaimDetailResponse>.NotFound("Insurance claim not found");

                // Authorization check: Only the renter or manager/admin can view
                if (!roles.Contains("MANAGER") && !roles.Contains("ADMIN") && insuranceClaim.RenterId != userId)
                {
                    return ResultResponse<InsuranceClaimDetailResponse>.Forbidden(
                        "You do not have permission to view this insurance claim");
                }

                // Get incident images from Media table
                var incidentImages = await _unitOfWork.GetMediaRepository()
                    .Query()
                    .Where(m => m.DocNo == id && m.EntityType == "InsuranceClaim")
                    .Select(m => m.FileUrl)
                    .ToListAsync();

                // Map to response
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

        public async Task<ResultResponse<List<InsuranceClaimResponse>>> GetMyInsuranceClaims()
        {
            try
            {
                // Get current renter
                var renterId = Guid.Parse(_currentUserService.UserId!);

                // Get all insurance claims for this renter
                var insuranceClaims = await _unitOfWork.GetInsuranceClaimRepository()
                    .GetInsuranceClaimsByRenterIdAsync(renterId);

                var response = _mapper.Map<List<InsuranceClaimResponse>>(insuranceClaims);

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
    }
}
