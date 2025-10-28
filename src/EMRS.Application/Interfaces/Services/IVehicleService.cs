using EMRS.Application.Common;
using EMRS.Application.DTOs.RentalPricingDTOs;
using EMRS.Application.DTOs.VehicleDTOs;
using EMRS.Application.DTOs.VehicleModelDTOs;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services;

public interface IVehicleService
{
    Task<ResultResponse<VehicleResponse>> CreateVehicle(CreateVehicleRequest createVehicleRequest);
    Task<ResultResponse<RentalPricingResponse>> CreateRentalPricing(CreateRentalPricingRequest createRentalPricingRequest);
    Task<ResultResponse<VehicleModelResponse>> CreateVehicleModel(VehicleModelCreateRequest createVehicleRequest);
    Task<ResultResponse<PaginationResult<List<VehicleListResponse>>>> GetAllVehicleAsync(VehicleSearchRequest vehicleSearchRequest, int PageSize, int PageNum);
    Task<ResultResponse<VehicleModelResponse>> GetVehicleModelByIdAsync(Guid vehicleModelId);
    Task<ResultResponse<List<VehicleModelListResponse>>> GetAllVehicleModel();
    Task<ResultResponse<VehicleResponse>> UpdateVehicleByIdAsync(VehicleUpdateRequest Updatingvehicle);
}
