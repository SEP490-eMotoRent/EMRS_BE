using AutoMapper;
using AutoMapper.QueryableExtensions;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.RentalPricingDTOs;
using EMRS.Application.DTOs.RenterDTOs;
using EMRS.Application.DTOs.VehicleDTOs;
using EMRS.Application.DTOs.VehicleModelDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class VehicleService:IVehicleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VehicleService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper= mapper;
    }


    public async Task<ResultResponse<VehicleResponse>> CreateVehicle(CreateVehicleRequest createVehicleRequest)
    {
        var modelTask = await _unitOfWork.GetVehicleModelRepository()
     .FindByIdAsync(createVehicleRequest.VehicleModelId);

        var branchTask = await _unitOfWork.GetBranchRepository()
            .FindByIdAsync(createVehicleRequest.BranchId);

      

        if (modelTask == null || branchTask == null)
        {
            return ResultResponse<VehicleResponse>.Failure("Branch or Model not exist");
        }

        var vehicle = new Vehicle
       {
           LicensePlate = createVehicleRequest.LicensePlate,
           Color = createVehicleRequest.Color,
           YearOfManufacture = createVehicleRequest.YearOfManufacture,
           CurrentOdometerKm = createVehicleRequest.CurrentOdometerKm,
           BatteryHealthPercentage = createVehicleRequest.BatteryHealthPercentage,
           Status = createVehicleRequest.Status,
           LastMaintenanceDate = createVehicleRequest.LastMaintenanceDate,
           NextMaintenanceDue = createVehicleRequest.NextMaintenanceDue,
           PurchaseDate = createVehicleRequest.PurchaseDate,
           Description = createVehicleRequest.Description,
           VehicleModelId = createVehicleRequest.VehicleModelId,
           BranchId = createVehicleRequest.BranchId
       };
        await _unitOfWork.GetVehicleRepository().AddAsync(vehicle);
        await _unitOfWork.SaveChangesAsync();
       
        VehicleResponse vehicleResponse = _mapper.
            Map<VehicleResponse>(await _unitOfWork
            .GetVehicleRepository().GetVehicleWithReferencesAsync(vehicle.Id,vehicle.VehicleModelId));
        return ResultResponse<VehicleResponse>.SuccessResult("Vehicle created successfully.", vehicleResponse);
    }
    public async Task<ResultResponse<RentalPricingResponse>> CreateRentalPricing(CreateRentalPricingRequest createRentalPricingRequest)
    {
        try
        {
            var rentalPricing = new RentalPricing
            {
                ExcessKmPrice = createRentalPricingRequest.ExcessKmPrice,
                RentalPrice = createRentalPricingRequest.RentalPrice,
            };
            await _unitOfWork.GetRentalPricingRepository().AddAsync(rentalPricing);
            await _unitOfWork.SaveChangesAsync();
            RentalPricingResponse rentalPricingResponse = _mapper.Map<RentalPricingResponse>(rentalPricing);
            return ResultResponse<RentalPricingResponse>.SuccessResult("RentalPricing created", rentalPricingResponse);
        }
        catch (Exception ex) {
            return ResultResponse<RentalPricingResponse>.Failure($"An error occurred while registering the user: {ex.Message}");

        }
    }
    public async Task<ResultResponse<VehicleModelResponse>> CreateVehicleModel(CreateVehicleModelRequest createVehicleModelRequest)
    {
        var rentalpricingTask = _unitOfWork.GetRentalPricingRepository()
           .FindByIdAsync(createVehicleModelRequest.RentalPricingId);
        if ( rentalpricingTask.Result == null)
        {
            return ResultResponse<VehicleModelResponse>.Failure("RentalPrice not exist");
        }

        var vehicle = new VehicleModel
        {
            BatteryCapacityKwh = createVehicleModelRequest.BatteryCapacityKwh,
            Category = createVehicleModelRequest.Category,
            Description = createVehicleModelRequest.Description,
            MaxSpeedKmh = createVehicleModelRequest.MaxSpeedKmh,
            ModelName = createVehicleModelRequest.ModelName,
            RentalPricingId = createVehicleModelRequest.RentalPricingId,

        };
        await _unitOfWork.GetVehicleModelRepository().AddAsync(vehicle);
        await _unitOfWork.SaveChangesAsync();
        VehicleModelResponse vehicleModelResponse = _mapper.Map<VehicleModelResponse>(vehicle);
        return ResultResponse<VehicleModelResponse>.SuccessResult("Vehicle created successfully.", vehicleModelResponse);
    }

    public async Task<ResultResponse<List<VehicleResponse>>> GetAllVehicles()
    {
        var repo = _unitOfWork.GetVehicleRepository();

        if (await repo.IsEmptyAsync())
            return ResultResponse<List<VehicleResponse>>.SuccessResult("No vehicles found.", new());

        var vehicles = await repo.Query()
            .ProjectTo<VehicleResponse>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return ResultResponse<List<VehicleResponse>>.SuccessResult("Vehicles retrieved successfully.", vehicles);


    }
}
