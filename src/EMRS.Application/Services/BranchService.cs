using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.VehicleModelDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class BranchService:IBranchService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IMapper _mapper;   
    public BranchService(IUnitOfWork unitOfWork,IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<ResultResponse<BranchResponse>> CreateABranch(CreateBranchRequest createBranchRequest)
    {
        try
        {
            var branch = new Branch
            {
                Address = createBranchRequest.Address,
                BranchName = createBranchRequest.BranchName,
                City = createBranchRequest.City,
                Email = createBranchRequest.Email,
                Phone = createBranchRequest.Phone,
                Latitude = createBranchRequest.Latitude,
                Longitude = createBranchRequest.Longitude,
                OpeningTime = createBranchRequest.OpeningTime,
                ClosingTime = createBranchRequest.ClosingTime
            };
            await _unitOfWork.GetBranchRepository().AddAsync(branch);
            await _unitOfWork.SaveChangesAsync();
            BranchResponse branchResponse = _mapper.Map<BranchResponse>(branch);
            return ResultResponse<BranchResponse>.SuccessResult("Branch created successfully.", branchResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<BranchResponse>.Failure("An error occurred while creating the branch: " + ex.Message);
        }
        


    }
    public async Task<ResultResponse<List<BranchResponse>>> GetAllBranches()
    {
        try
        {
            var branches = await _unitOfWork.GetBranchRepository().GetAllAsync();
            var branchResponses = _mapper.Map<List<BranchResponse>>(branches);
            return ResultResponse<List<BranchResponse>>.SuccessResult("Branches retrieved successfully.", branchResponses);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<BranchResponse>>.Failure("An error occurred while retrieving branches: " + ex.Message);
        }
    }
    public async Task<ResultResponse<List<BranchModelDetailResponse>>> GetAllBranchesWithSameModelIdAsync(Guid vehicleModelId)
    {
        try
        {
           
            var branches = await _unitOfWork.GetBranchRepository()
                .GetBranchByVehicleModelIdAsync(vehicleModelId);

            if (branches == null || !branches.Any())
                return ResultResponse<List<BranchModelDetailResponse>>
                    .Failure("No branches found for the given vehicle model.");

            var branchResponses = branches.Select(b => new BranchModelDetailResponse
            {
                Id = b.Id,
                BranchName = b.BranchName,
                Address = b.Address,
                City = b.City,
                Phone = b.Phone,
                Email = b.Email,
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                OpeningTime = b.OpeningTime,
                ClosingTime = b.ClosingTime,
                VehicleCount =  b.Vehicles.Count(v =>
                v.VehicleModelId == vehicleModelId &&
                v.Status == VehicleStatusEnum.Available.ToString())
            }).ToList();

            return ResultResponse<List<BranchModelDetailResponse>>
                .SuccessResult("Branches retrieved successfully.", branchResponses);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<BranchModelDetailResponse>>
                .Failure("An error occurred while retrieving branches: " + ex.Message);
        }
    }
    public async Task<ResultResponse<List<BranchResponse>>> GetNearbyBranchesAsync(
     double lat, double lon, double radiusKm)
    {
        try
        {
         
            var latRange = radiusKm / 111.0; 
            var lonRange = radiusKm / (111.0 * Math.Cos(lat * Math.PI / 180));

           
            var branches = await _unitOfWork.GetBranchRepository()
                .GetBranchesInBoundingBoxAsync(lat, lon, latRange, lonRange);

           
            var nearby = branches.Select(b => new BranchResponse
            {
              Id=b.Id,
              Latitude=b.Latitude,
              Longitude=b.Longitude,
              Address=b.Address,
              BranchName=b.BranchName,
              City=b.City,
              ClosingTime=b.ClosingTime,
              Email=b.Email,
              OpeningTime=b.OpeningTime,
              Phone=b.Phone,
              
            }).ToList();

            return ResultResponse<List<BranchResponse>>.SuccessResult("Here are the nearby Branches",nearby);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<BranchResponse>>.Failure("Error: " + ex.Message);
        }
    }

    public async Task<ResultResponse<List<BranchSearchListResponse>>>
        SearchWithTimeSpanForBranch(BranchSearchRequest branchSearchRequest)
    {
        try
        {
            var branch = await _unitOfWork.GetBranchRepository()
                .SearchBranchWithAvailableModelsAsync(branchSearchRequest);

            var vehicleModelsIds = branch.Select(v => v.Id).ToList();
            
            var listresponse = branch.Select(v =>
            {
                return new BranchSearchListResponse
                {
                 Id = v.Id,
                 Address = v.Address,
                 BranchName = v.BranchName,
                 City = v.City,
                 ClosingTime = v.ClosingTime,
                 Email = v.Email,
                 Latitude = v.Latitude,
                 Longitude = v.Longitude,
                 OpeningTime = v.OpeningTime,
                 Phone= v.Phone,
                };
            }).ToList();
           
            return ResultResponse<List<BranchSearchListResponse>>.SuccessResult("Branches retrieved successfully.", listresponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<BranchSearchListResponse>>.Failure($"An error occurred while retrieving branches: {ex.Message}");
        }
    }


    public async Task<ResultResponse<BranchResponse>> UpdateBranch(
    Guid branchId,
    UpdateBranchRequest updateBranchRequest)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

           
            var branch = await _unitOfWork.GetBranchRepository().FindByIdAsync(branchId);
            if (branch == null)
                return ResultResponse<BranchResponse>.NotFound("Branch not found");

            
            if (branch.IsDeleted)
                return ResultResponse<BranchResponse>.Failure("Branch has been deleted");

            
            if (!string.IsNullOrEmpty(updateBranchRequest.BranchName))
                branch.BranchName = updateBranchRequest.BranchName;

            if (!string.IsNullOrEmpty(updateBranchRequest.Address))
                branch.Address = updateBranchRequest.Address;

            if (!string.IsNullOrEmpty(updateBranchRequest.City))
                branch.City = updateBranchRequest.City;

            if (!string.IsNullOrEmpty(updateBranchRequest.Phone))
                branch.Phone = updateBranchRequest.Phone;

            if (!string.IsNullOrEmpty(updateBranchRequest.Email))
                branch.Email = updateBranchRequest.Email;

            if (updateBranchRequest.Latitude.HasValue)
                branch.Latitude = updateBranchRequest.Latitude.Value;

            if (updateBranchRequest.Longitude.HasValue)
                branch.Longitude = updateBranchRequest.Longitude.Value;

            if (!string.IsNullOrEmpty(updateBranchRequest.OpeningTime))
                branch.OpeningTime = updateBranchRequest.OpeningTime;

            if (!string.IsNullOrEmpty(updateBranchRequest.ClosingTime))
                branch.ClosingTime = updateBranchRequest.ClosingTime;

            
            _unitOfWork.GetBranchRepository().Update(branch);

            
            await _unitOfWork.SaveChangesAsync();

           
            await _unitOfWork.CommitAsync();

           
            var branchResponse = _mapper.Map<BranchResponse>(branch);

            return ResultResponse<BranchResponse>.SuccessResult(
                "Branch updated successfully",
                branchResponse);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<BranchResponse>.Failure(
                $"An error occurred while updating branch: {ex.Message}");
        }
    }

    public async Task<ResultResponse<bool>> DeleteBranch(Guid branchId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            
            var branch = await _unitOfWork.GetBranchRepository().FindByIdAsync(branchId);
            if (branch == null)
                return ResultResponse<bool>.NotFound("Branch not found");

            
            if (branch.IsDeleted)
                return ResultResponse<bool>.Failure("Branch has already been deleted");

            
            var hasActiveBookings = await _unitOfWork.GetBookingRepository()
                .Query()
                .AnyAsync(b =>
                    (b.HandoverBranchId == branchId || b.ReturnBranchId == branchId) &&
                    (b.BookingStatus == BookingStatusEnum.Booked.ToString() ||
                     b.BookingStatus == BookingStatusEnum.Renting.ToString()) &&
                    !b.IsDeleted);

            if (hasActiveBookings)
                return ResultResponse<bool>.Failure(
                    "Cannot delete branch with active bookings. Please complete or cancel all active bookings first");

            
            var hasVehicles = await _unitOfWork.GetVehicleRepository()
                .Query()
                .AnyAsync(v => v.BranchId == branchId && !v.IsDeleted);

            if (hasVehicles)
                return ResultResponse<bool>.Failure(
                    "Cannot delete branch with vehicles. Please transfer all vehicles to other branches first");

            
            branch.Delete();

            
            _unitOfWork.GetBranchRepository().Update(branch);

           
            await _unitOfWork.SaveChangesAsync();

            
            await _unitOfWork.CommitAsync();

            return ResultResponse<bool>.SuccessResult(
                "Branch deleted successfully",
                true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<bool>.Failure(
                $"An error occurred while deleting branch: {ex.Message}");
        }
    }

}
