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
using EMRS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class VehicleService:IVehicleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;

    public VehicleService(ICloudinaryService cloudinaryService,IUnitOfWork unitOfWork, IMapper mapper)
    {
        _cloudinaryService = cloudinaryService;
        _unitOfWork = unitOfWork;
        _mapper= mapper;
    }


    public async Task<ResultResponse<VehicleResponse>> CreateVehicle(CreateVehicleRequest createVehicleRequest)
    {
        if(createVehicleRequest.ImageFiles== null || createVehicleRequest.ImageFiles.Count == 0)
        {
            return ResultResponse<VehicleResponse>.Failure("Image file is required.");
        }
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
           Status = VehicleStatusEnum.Unavailable.ToString(),
           LastMaintenanceDate = createVehicleRequest.LastMaintenanceDate,
           NextMaintenanceDue = createVehicleRequest.NextMaintenanceDue,
           PurchaseDate = createVehicleRequest.PurchaseDate,
           Description = createVehicleRequest.Description,
           VehicleModelId = createVehicleRequest.VehicleModelId,
           BranchId = createVehicleRequest.BranchId
       };
        //upload async multiple files
        
        var uploadTasks= createVehicleRequest.ImageFiles.Select(async file =>
        {

            var url = await _cloudinaryService.UploadImageFileAsync(
                file,
                $"img_{Generator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                "vehicle_images"
                );
            return new Media
            {
                EntityType = MediaEntityTypeEnum.Vehicle.ToString(),
                FileUrl = url,
                DocNo = vehicle.Id,
                MediaType = MediaTypeEnum.Image.ToString(),
            };
        }).ToList();
        //wait for all task to complete
        List<Media> medias = (await Task.WhenAll(uploadTasks)).ToList();
     
        await _unitOfWork.GetMediaRepository().AddRangeAsync(medias);
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
    public async Task<ResultResponse<VehicleModelResponse>> CreateVehicleModel(VehicleModelCreateRequest createVehicleModelRequest)
    {
        var rentalpricingTask = _unitOfWork.GetRentalPricingRepository()
           .FindByIdAsync(createVehicleModelRequest.RentalPricingId);
        if ( rentalpricingTask.Result == null)
        {
            return ResultResponse<VehicleModelResponse>.Failure("RentalPrice not exist");
        }
        if (createVehicleModelRequest.ImageFiles == null || createVehicleModelRequest.ImageFiles.Count == 0)
        {
            return ResultResponse<VehicleModelResponse>.Failure("Image file is required.");
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
        var uploadTasks = createVehicleModelRequest.ImageFiles.Select(async file =>
        {

            var url = await _cloudinaryService.UploadImageFileAsync(
                file,
                $"img_{Generator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                "Images"
                );
            return new Media
            {
                EntityType = MediaEntityTypeEnum.VehicleModel.ToString(),
                FileUrl = url,
                DocNo = vehicle.Id,
                MediaType = MediaTypeEnum.Image.ToString(),
            };
        }).ToList();
        List<Media> medias = (await Task.WhenAll(uploadTasks)).ToList();

        await _unitOfWork.GetMediaRepository().AddRangeAsync(medias);
        await _unitOfWork.GetVehicleModelRepository().AddAsync(vehicle);
        await _unitOfWork.SaveChangesAsync();
        VehicleModelResponse vehicleModelResponse = _mapper.Map<VehicleModelResponse>(vehicle);
        return ResultResponse<VehicleModelResponse>.SuccessResult("Vehicle model created successfully.", vehicleModelResponse);
    }
    public async Task<ResultResponse<PaginationResult<List<VehicleListResponse>>>> GetAllVehicleAsync(VehicleSearchRequest vehicleSearchRequest, int PageSize, int PageNum)
    {
        try
        {
            var vehicles = await _unitOfWork.GetVehicleRepository()
                .GetVehicleListWithReferencesAsync(vehicleSearchRequest, PageSize, PageNum);

            var vehicleIds = vehicles.Items.Select(v => v.Id).ToList();
            var medias = await _unitOfWork.GetMediaRepository().Query().Where(a =>
                  a.EntityType == MediaEntityTypeEnum.Vehicle.ToString() && vehicleIds.Contains(a.DocNo))
                .ToListAsync();

            var mediaDict = medias
                .GroupBy(a => a.DocNo)
                .ToDictionary(g => g.Key, g => g.ToList());
            var listresponse = vehicles.Items.Select(v =>
            {

                return new VehicleListResponse
                {
                    BatteryHealthPercentage = v.BatteryHealthPercentage,
                    Color = v.Color,
                    Id = v.Id,
                    LicensePlate = v.LicensePlate,
                    NextMaintenanceDue = v.NextMaintenanceDue,
                    rentalPricing = v.VehicleModel.RentalPricing.RentalPrice,
                    Status = v.Status,
                    CurrentOdometerKm = v.CurrentOdometerKm,
                    FileUrl = mediaDict.TryGetValue(v.Id, out var mediaL)
                    ? mediaL.Select(m => m.FileUrl).ToList()
                    : new List<string>()


                };
            }).ToList();
            var response = new PaginationResult<List<VehicleListResponse>>
            {
                CurrentPage = vehicles.CurrentPage,
                PageSize = vehicles.PageSize,
                TotalItems = vehicles.TotalItems,
                TotalPages = vehicles.TotalPages,
                Items = listresponse
            };
            return ResultResponse<PaginationResult<List<VehicleListResponse>>>.SuccessResult("Vehicles retrieved successfully.", response);
        }
        catch (Exception ex)
        {
            return ResultResponse<PaginationResult<List<VehicleListResponse>>>.Failure($"An error occurred while retrieving vehicles: {ex.Message}");
        }
    }
    public async Task<ResultResponse<VehicleResponse>> UpdateVehicleByIdAsync(VehicleUpdateRequest Updatingvehicle)
    {
        try
        {
            var vehicle = await _unitOfWork.GetVehicleRepository()
                .FindByIdAsync(Updatingvehicle.VehicleId);
            if (vehicle == null)
            {
                return ResultResponse<VehicleResponse>.NotFound("Vehicle not found.");
            }
            vehicle.Color = Updatingvehicle.Color;
            vehicle.CurrentOdometerKm = Updatingvehicle.CurrentOdometerKm;
            vehicle.BatteryHealthPercentage = Updatingvehicle.BatteryHealthPercentage;
            vehicle.Status = Updatingvehicle.Status.ToString();
            vehicle.LastMaintenanceDate = Updatingvehicle.LastMaintenanceDate;
            vehicle.NextMaintenanceDue = Updatingvehicle.NextMaintenanceDue;
            vehicle.BatteryHealthPercentage = Updatingvehicle.BatteryHealthPercentage;
            vehicle.PurchaseDate = Updatingvehicle.PurchaseDate;
            vehicle.Description = Updatingvehicle.Description;

            _unitOfWork.GetVehicleRepository().Update(vehicle);
            VehicleResponse vehicleResponse = _mapper.Map<VehicleResponse>(vehicle);
            await _unitOfWork.SaveChangesAsync();
            return ResultResponse<VehicleResponse>.SuccessResult("Vehicle retrieved successfully.", vehicleResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<VehicleResponse>.Failure($"An error occurred while retrieving the vehicle: {ex.Message}");
        }
    }

    public async Task<ResultResponse<List<VehicleModelListResponse>>> GetAllVehicleModel()
    {
        try
        {
            var repo = await _unitOfWork.GetVehicleModelRepository().GetVehicleModelsWithReferencesAsync();
            var medias =  await _unitOfWork.GetMediaRepository().Query().Where(a=>
                                                                       a.EntityType==MediaEntityTypeEnum.VehicleModel.ToString()).ToListAsync();
            var mediaDict = medias
   .GroupBy(m => m.DocNo)
   .ToDictionary(g => g.Key, g => g.First().FileUrl);

            if ( !repo.Any())
                return ResultResponse<List<VehicleModelListResponse>>.SuccessResult("No vehicles found.",new List<VehicleModelListResponse>());
            var response = repo.Select( v =>
            {
                mediaDict.TryGetValue(v.Id, out var mediaUrl);
                return new VehicleModelListResponse
                {
                    VehicleModelId = v.Id,
                    MaxRangeKm= v.MaxRangeKm,
                    ModelName = v.ModelName,
                    RentalPrice = v.RentalPricing.RentalPrice,
                    Category = v.Category,
                    BatteryCapacityKwh = v.BatteryCapacityKwh,
                    ImageUrl = mediaUrl,
                    AvailableColors = v.Vehicles
                    .Select(v => new ColorResponse { ColorName = v.Color })
                    .DistinctBy(c => c.ColorName) 
                    .ToList()

                };
            }).ToList();

         
            return ResultResponse<List<VehicleModelListResponse>>.SuccessResult("Vehicles retrieved successfully.", response);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<VehicleModelListResponse>>.Failure($"An error occurred while retrieving vehicles: {ex.Message}");

        }
    }
    public async Task<ResultResponse<VehicleModelResponse>> GetVehicleModelByIdAsync(Guid vehicleModelId)
    {
        var vehicleModel = await _unitOfWork.GetVehicleModelRepository()
            .FindByIdAsync(vehicleModelId);
        if (vehicleModel == null)
        {
           return ResultResponse<VehicleModelResponse>.NotFound("Vehicle model not found.");
        }
        VehicleModelResponse  vehicleModelResponse= _mapper.Map<VehicleModelResponse>(vehicleModel);
        return ResultResponse<VehicleModelResponse>.SuccessResult("Vehicle model retrieved successfully.", vehicleModelResponse);
    }




   
}
