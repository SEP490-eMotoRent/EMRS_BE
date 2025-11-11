using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.HolidayPricingDTOs;
using EMRS.Application.Interfaces.Repositories;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services
{
    public class HolidayPricingService:IHolidayPricingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public HolidayPricingService( IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private HolidayPricingResponse MapToResponse(HolidayPricing entity) => new HolidayPricingResponse
        {
            Id = entity.Id,
            HolidayName = entity.HolidayName,
            HolidayDate = entity.HolidayDate,
            PriceMultiplier = entity.PriceMultiplier,
            Description = entity.Description,
            IsActive = entity.IsActive,
            IsDeleted = entity.IsDeleted,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            DeletedAt = entity.DeletedAt
        };

        public async Task<ResultResponse<List<HolidayPricingResponse>>> GetAllAsync()
        {
            try
            {
                var list = await _unitOfWork.GetHolidayPricingRepository().GetAllAsync();
                var activeList = list.Where(h => !h.IsDeleted).ToList();

                if (!activeList.Any())
                    return ResultResponse<List<HolidayPricingResponse>>.NotFound("No holiday pricing found");

                var response = activeList.Select(h => new HolidayPricingResponse
                {
                    Id = h.Id,
                    HolidayName = h.HolidayName,
                    HolidayDate = h.HolidayDate,
                    PriceMultiplier = h.PriceMultiplier,
                    Description = h.Description,
                    IsActive = h.IsActive,
                    CreatedAt= h.CreatedAt,
                    DeletedAt  = h.DeletedAt,
                    UpdatedAt = h.UpdatedAt,
                    IsDeleted = h.IsDeleted
                }).ToList();

                return ResultResponse<List<HolidayPricingResponse>>.SuccessResult("Holiday pricing list retrieved", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<HolidayPricingResponse>>.Failure($"Error fetching holiday pricing: {ex.Message}");
            }
        }


        public async Task<ResultResponse<HolidayPricingResponse>> GetByIdAsync(Guid id)
        {
            try
            {
                var entity = await _unitOfWork.GetHolidayPricingRepository().FindByIdAsync(id);
                if (entity == null || entity.IsDeleted)
                    return ResultResponse<HolidayPricingResponse>.NotFound("Holiday pricing not found");

                return ResultResponse<HolidayPricingResponse>.SuccessResult("Holiday pricing retrieved", MapToResponse(entity));
            }
            catch (Exception ex)
            {
                return ResultResponse<HolidayPricingResponse>.Failure($"Error retrieving holiday pricing: {ex.Message}");
            }
        }

        public async Task<ResultResponse<HolidayPricingResponse>> CreateAsync(HolidayPricingCreateRequest request)
        {
            try
            {
                var entity = new HolidayPricing
                {
                    Id = Guid.NewGuid(),
                    HolidayName = request.HolidayName,
                    HolidayDate = DateTimeHelper.NormalizeToUtc(request.HolidayDate),
                    PriceMultiplier = request.PriceMultiplier,
                    Description = request.Description ,
                    IsActive = request.IsActive
                };

                await _unitOfWork.GetHolidayPricingRepository().AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return ResultResponse<HolidayPricingResponse>.SuccessResult("Holiday pricing created successfully", MapToResponse(entity));
            }
            catch (Exception ex)
            {
                return ResultResponse<HolidayPricingResponse>.Failure($"Error creating holiday pricing: {ex.Message}");
            }
        }

        public async Task<ResultResponse<HolidayPricingResponse>> UpdateAsync(HolidayPricingUpdateRequest request)
        {
            try
            {
                var entity = await _unitOfWork.GetHolidayPricingRepository().FindByIdAsync(request.Id);
                if (entity == null || entity.IsDeleted)
                    return ResultResponse<HolidayPricingResponse>.NotFound("Holiday pricing not found");

                entity.HolidayName = request.HolidayName;
                entity.HolidayDate = DateTimeHelper.NormalizeToUtc(request.HolidayDate);
                entity.PriceMultiplier = request.PriceMultiplier;
                entity.Description = request.Description ?? string.Empty;
                entity.IsActive = request.IsActive;
                _unitOfWork.GetHolidayPricingRepository().Update(entity);
                await _unitOfWork.SaveChangesAsync();

                return ResultResponse<HolidayPricingResponse>.SuccessResult("Holiday pricing updated successfully", MapToResponse(entity));
            }
            catch (Exception ex)
            {
                return ResultResponse<HolidayPricingResponse>.Failure($"Error updating holiday pricing: {ex.Message}");
            }
        }

        public async Task<ResultResponse<bool>> DeleteAsync(Guid id)
        {
            try
            {
                var entity = await _unitOfWork.GetHolidayPricingRepository().FindByIdAsync(id);
                if (entity == null || entity.IsDeleted)
                    return ResultResponse<bool>.NotFound("Holiday pricing not found");

                entity.Delete(); 
                _unitOfWork.GetHolidayPricingRepository().Update(entity);
                await _unitOfWork.SaveChangesAsync();

                return ResultResponse<bool>.SuccessResult("Holiday pricing deleted successfully", true);
            }
            catch (Exception ex)
            {
                return ResultResponse<bool>.Failure($"Error deleting holiday pricing: {ex.Message}");
            }
        }
    }
}
