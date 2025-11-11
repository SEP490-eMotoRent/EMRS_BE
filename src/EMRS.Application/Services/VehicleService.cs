using AutoMapper;
using AutoMapper.QueryableExtensions;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.BranchDTOs;
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
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class VehicleService:IVehicleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IProtrackService _protrackService;
    private readonly ICloudinaryService _cloudinaryService;


    public VehicleService(IProtrackService protrackService,ICloudinaryService cloudinaryService,IUnitOfWork unitOfWork, IMapper mapper)
    {
        _protrackService= protrackService;
        _cloudinaryService = cloudinaryService;
        _unitOfWork = unitOfWork;
        _mapper= mapper;
    }

    public async Task<ResultResponse<VehicleDetailResponse>> GetVehicleDetailAsync(Guid VehicleId)
    {

        try
        {
            var vehicle= await _unitOfWork.GetVehicleRepository().GetVehicleWithReferences2Async(VehicleId);
            var branch =vehicle.Branch;
            var vehicleModel = vehicle.VehicleModel;
            var rentalPricing=vehicleModel.RentalPricing;
            VehicleDetailResponse vehicleDetailResponse = new VehicleDetailResponse
            {

                Id = vehicle.Id,
                BatteryHealthPercentage = vehicle.BatteryHealthPercentage,
                Color = vehicle.Color,
                CurrentOdometerKm = vehicle.CurrentOdometerKm,
                Description = vehicle.Description,
                LastMaintenanceDate = vehicle.LastMaintenanceDate,
                LicensePlate = vehicle.LicensePlate,
                NextMaintenanceDue = vehicle.NextMaintenanceDue,
                PurchaseDate = vehicle.PurchaseDate,
                Status = vehicle.Status,
                YearOfManufacture = vehicle.YearOfManufacture,
                branch = new BranchResponse
                {
                    Id = branch.Id,
                    Address = branch.Address,
                    BranchName = branch.BranchName,
                    City = branch.City,
                    ClosingTime = branch.ClosingTime,
                    Email = branch.Email,
                    Latitude = branch.Latitude,
                    Longitude = branch.Longitude,
                    OpeningTime = branch.OpeningTime,
                    Phone = branch.Phone,
                },
                vehicleModel = new VehicleModelReponseWithRentalPricing
                {
                    Id = vehicleModel.Id,
                    BatteryCapacityKwh = vehicleModel.BatteryCapacityKwh,
                    Category = vehicleModel.Category,
                    Description = vehicleModel.Description,
                    MaxRangeKm = vehicleModel.MaxRangeKm,
                    MaxSpeedKmh = vehicleModel.MaxSpeedKmh,
                    ModelName = vehicleModel.ModelName,
                    RentalPricingResponse = new RentalPricingResponse
                    {
                        Id = rentalPricing.Id,
                        ExcessKmPrice = rentalPricing.ExcessKmPrice,
                        RentalPrice = rentalPricing.RentalPrice,
                    }
                }
            };
            return ResultResponse<VehicleDetailResponse>.SuccessResult("Vehicle created successfully.", vehicleDetailResponse);

        }
        catch (Exception ex)
        {
            return ResultResponse<VehicleDetailResponse>.Failure($"An error occurred while registering the user: {ex.Message}");

        }

    }
    public async Task<ResultResponse<VehicleResponse>> CreateVehicle(CreateVehicleRequest createVehicleRequest)
    {
        try
        {
            if (createVehicleRequest.ImageFiles == null || createVehicleRequest.ImageFiles.Count == 0)
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
                YearOfManufacture = DateTimeHelper.NormalizeToUtc(createVehicleRequest.YearOfManufacture),
                CurrentOdometerKm = createVehicleRequest.CurrentOdometerKm,
                BatteryHealthPercentage = createVehicleRequest.BatteryHealthPercentage,
                Status = VehicleStatusEnum.Unavailable.ToString(),
                LastMaintenanceDate = DateTimeHelper.NormalizeToUtc(createVehicleRequest.LastMaintenanceDate),
                NextMaintenanceDue = DateTimeHelper.NormalizeToUtc(createVehicleRequest.NextMaintenanceDue),
                PurchaseDate = DateTimeHelper.NormalizeToUtc(createVehicleRequest.PurchaseDate),
                Description = createVehicleRequest.Description,
                VehicleModelId = createVehicleRequest.VehicleModelId,
                BranchId = createVehicleRequest.BranchId
            };
            //upload async multiple files

            var uploadTasks = createVehicleRequest.ImageFiles.Select(async file =>
            {

                var url = await _cloudinaryService.UploadImageFileAsync(
                    file,
                    $"img_{Generator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                    "Vehicle"
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
                .GetVehicleRepository().GetVehicleWithReferencesAsync(vehicle.Id, vehicle.VehicleModelId));
            return ResultResponse<VehicleResponse>.SuccessResult("Vehicle created successfully.", vehicleResponse);
        }catch(Exception ex)
        {
            return ResultResponse<VehicleResponse>.Failure($"An error occurred while  {ex.Message}");

        }
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
                var vehicleModel = v.VehicleModel;
                var rentalPricing= vehicleModel.RentalPricing;
                return new VehicleListResponse
                {
                    BatteryHealthPercentage = v.BatteryHealthPercentage,
                    Color = v.Color,
                    Id = v.Id,
                    LicensePlate = v.LicensePlate,
                    NextMaintenanceDue = v.NextMaintenanceDue,
                    Status = v.Status,
                    CurrentOdometerKm = v.CurrentOdometerKm,
                    FileUrl = mediaDict.TryGetValue(v.Id, out var mediaL)
                    ? mediaL.Select(m => m.FileUrl).ToList()
                    : new List<string>(),
                    rentalPricing=new RentalPricingResponse
                    {
                        Id=rentalPricing.Id,
                        ExcessKmPrice=rentalPricing.ExcessKmPrice,
                        RentalPrice=rentalPricing.RentalPrice,
                    },
                    vehicleModel= new VehicleModelResponse
                    {
                        Id=vehicleModel.Id,
                        BatteryCapacityKwh=vehicleModel.BatteryCapacityKwh,
                        Category=vehicleModel.Category,
                        Description=vehicleModel.Description,
                        MaxRangeKm=vehicleModel.MaxRangeKm,
                        MaxSpeedKmh=vehicleModel.MaxSpeedKmh,
                        ModelName = vehicleModel.ModelName  
                        
                    }

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
    public async Task<ResultResponse<VehicleModelResponse>> CreateVehicleModel(VehicleModelCreateRequest createVehicleModelRequest)
    {
        var rentalpricingTask = _unitOfWork.GetRentalPricingRepository()
           .FindByIdAsync(createVehicleModelRequest.RentalPricingId);
        if (rentalpricingTask.Result == null)
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
                "VehicleModel"
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

    public async Task<ResultResponse<PaginationResult<List<VehicleModelListResponse>>>>
        SearchWithTimeSpanForVehicleModels(VehicleModelSearchRequest vehiclemodelSearchRequest, int PageSize, int PageNum)
    {
        try
        {
            var vehiclesModels =  await _unitOfWork.GetVehicleModelRepository()
                .SearchAvailableModelsPaginationAsync(vehiclemodelSearchRequest, PageSize, PageNum);

            var vehicleModelsIds = vehiclesModels.Items.Select(v => v.Id).ToList();
            var medias = await _unitOfWork.GetMediaRepository().Query().Where(a =>
                  a.EntityType == MediaEntityTypeEnum.VehicleModel.ToString() && vehicleModelsIds.Contains(a.DocNo))
                .ToListAsync();
            var currrentSaleForToday = await _unitOfWork.GetHolidayPricingRepository().GetHolidayByCurrentDateAsync();

            var mediaDict = medias
                .GroupBy(a => a.DocNo)
                .ToDictionary(g => g.Key, g => g.ToList());
           var listresponse = vehiclesModels.Items.Select(v =>
            {
                return new VehicleModelListResponse
                {
                    VehicleModelId = v.Id,
                    MaxRangeKm = v.MaxRangeKm,
                    ModelName = v.ModelName,
                    OriginalRentalPrice = currrentSaleForToday != null? v.RentalPricing.RentalPrice:0,
                    RentalPrice = v.RentalPricing.RentalPrice * (currrentSaleForToday != null ? currrentSaleForToday.PriceMultiplier : 1),

                    Category = v.Category,
                    BatteryCapacityKwh = v.BatteryCapacityKwh,
                    ImageUrl = mediaDict.TryGetValue(v.Id, out var mediaL)
                    ? mediaL.Select(m => m.FileUrl).FirstOrDefault()
                    : null,
                    AvailableColors = v.Vehicles
                    .Select(v => new ColorResponse { ColorName = v.Color })
                    .DistinctBy(c => c.ColorName)
                    .ToList()??new List<ColorResponse>()
                };
            }).ToList();
            var response = new PaginationResult<List<VehicleModelListResponse>>
            {
                CurrentPage = vehiclesModels.CurrentPage,
                PageSize = vehiclesModels.PageSize,
                TotalItems = vehiclesModels.TotalItems,
                TotalPages = vehiclesModels.TotalPages,
                Items = listresponse
            };
            return ResultResponse<PaginationResult<List<VehicleModelListResponse>>>.SuccessResult("Vehicles retrieved successfully.", response);
        }
        catch (Exception ex)
        {
            return ResultResponse<PaginationResult<List<VehicleModelListResponse>>>.Failure($"An error occurred while retrieving vehicles: {ex.Message}");
        }
    }
    public async Task<ResultResponse<List<VehicleModelListResponse>>>
        SearchWithTimeSpanForVehicleModelsNoPagination(VehicleModelSearchRequest vehiclemodelSearchRequest)
    {
        try
        {
            var vehiclesModels =  await _unitOfWork.GetVehicleModelRepository()
                .SearchAvailableModelsQuery(vehiclemodelSearchRequest).ToListAsync();
            var currrentSaleForToday = await _unitOfWork.GetHolidayPricingRepository().GetHolidayByCurrentDateAsync();
            var vehicleModelsIds = vehiclesModels.Select(v => v.Id).ToList();
            var medias = await _unitOfWork.GetMediaRepository().Query().Where(a =>
                  a.EntityType == MediaEntityTypeEnum.VehicleModel.ToString() && vehicleModelsIds.Contains(a.DocNo))
                .ToListAsync();

            var mediaDict = medias
                .GroupBy(a => a.DocNo)
                .ToDictionary(g => g.Key, g => g.ToList());
            var listresponse = vehiclesModels.Select(v =>
            {
                return new VehicleModelListResponse
                {
                    VehicleModelId = v.Id,
                    MaxRangeKm = v.MaxRangeKm,
                    ModelName = v.ModelName,
                    OriginalRentalPrice = currrentSaleForToday != null ? v.RentalPricing.RentalPrice : 0,
                    Category = v.Category,
                    RentalPrice=v.RentalPricing.RentalPrice * (currrentSaleForToday!=null ? currrentSaleForToday.PriceMultiplier :1),
                    BatteryCapacityKwh = v.BatteryCapacityKwh,
                    ImageUrl = mediaDict.TryGetValue(v.Id, out var mediaL)
                    ? mediaL.Select(m => m.FileUrl).FirstOrDefault()
                    : null,
                    AvailableColors = v.Vehicles
                    .Select(v => new ColorResponse { ColorName = v.Color })
                    .DistinctBy(c => c.ColorName)
                    .ToList() ?? new List<ColorResponse>()
                };
            }).ToList();
            
            return ResultResponse<List<VehicleModelListResponse>>.SuccessResult("Vehicles retrieved successfully.", listresponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<VehicleModelListResponse>>.Failure($"An error occurred while retrieving vehicles: {ex.Message}");
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
            vehicle.BranchId = Updatingvehicle.BranchId;
            vehicle.LicensePlate = Updatingvehicle.LicensePlate;
            vehicle.YearOfManufacture = Updatingvehicle.YearOfManufacture;
            vehicle.ProtrackAccount = Updatingvehicle.ProtrackAccount;
            vehicle.ProtrackPassword = _protrackService.EncryptPassword(Updatingvehicle.ProtrackPassword);
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
            var currrentSaleForToday = await _unitOfWork.GetHolidayPricingRepository().GetHolidayByCurrentDateAsync();

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
                    OriginalRentalPrice = currrentSaleForToday != null ? v.RentalPricing.RentalPrice : 0,
                    RentalPrice = v.RentalPricing.RentalPrice * (currrentSaleForToday != null ? currrentSaleForToday.PriceMultiplier : 1),

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
    public async Task<ResultResponse<VehicleModelDetailResponse>> GetVehicleModelByIdAsync(Guid vehicleModelId)
    {
        try
        {
            var vehicleModel = await _unitOfWork.GetVehicleModelRepository()
                .GetVehicleModelWithReferencesByIdAsync(vehicleModelId);
            var rentalPricing = vehicleModel.RentalPricing;
            var media = await _unitOfWork.GetMediaRepository().Query().Where(a =>
                 a.EntityType == MediaEntityTypeEnum.VehicleModel.ToString()&& a.DocNo==vehicleModelId)
               .ToListAsync();
            if (vehicleModel == null)
            {
                return ResultResponse<VehicleModelDetailResponse>.NotFound("Vehicle model not found.");
            }
            if (!Enum.GetNames(typeof(VehicleCategoryEnum)).Contains(vehicleModel.Category))
            {
                return ResultResponse<VehicleModelDetailResponse>.NotFound(
                    "Vehicle model has an invalid category."
                );
            }

            var depositAmountType = await _unitOfWork.GetConfigurationRepository().GetAllAsync();
            var configType = vehicleModel.Category switch
            {
                nameof(VehicleCategoryEnum.ECONOMY) => ConfigurationTypeEnum.EconomyDepositPrice,
                nameof(VehicleCategoryEnum.STANDARD) => ConfigurationTypeEnum.StandardDepositPrice,
                nameof(VehicleCategoryEnum.PREMIUM) => ConfigurationTypeEnum.PremiumDepositPrice,
                _ => (ConfigurationTypeEnum)(-1)
            };

            decimal depositAmountDecimal = 0;

            if ((int)configType != -1)
            {
                var config = await _unitOfWork.GetConfigurationRepository()
                    .Query()
                    .Where(c => c.Type == (int)configType)
                    .Select(c => c.Value)
                    .FirstOrDefaultAsync();

                decimal.TryParse(config, out depositAmountDecimal);
            }


            VehicleModelDetailResponse response = new VehicleModelDetailResponse
            {

                Id = vehicleModel.Id,
                Description = vehicleModel.Description,
                BatteryCapacityKwh = vehicleModel.BatteryCapacityKwh,
                Category = vehicleModel.Category,
                MaxRangeKm = vehicleModel.MaxRangeKm,
                MaxSpeedKmh = vehicleModel.MaxSpeedKmh,
                ModelName = vehicleModel.ModelName,
                RentalPricing = new RentalPricingResponse
                {
                    Id = rentalPricing.Id,
                    ExcessKmPrice = rentalPricing.ExcessKmPrice,
                    RentalPrice = rentalPricing.RentalPrice,
                },
                DepositAmount= depositAmountDecimal,
                images = media.Select(m=>m.FileUrl).ToList()
                

            };


            return ResultResponse<VehicleModelDetailResponse>.SuccessResult("Vehicle model retrieved successfully.", response);
        }
        catch (Exception ex)
        {
            return ResultResponse<VehicleModelDetailResponse>.Failure($"An error occurred while retrieving vehicles: {ex.Message}");

        }
    }
    public async Task<ResultResponse<VehicleTrackingResponse>> GetVehicleTrackingTokenAndSignature(Guid vehicleId)
    {
        try
        {
            var vehicle = await _unitOfWork.GetVehicleRepository().GetVehicleWithReferences2Async(vehicleId);
            if (vehicle == null)
            {
                return null;
            }
            var rentalPricing= vehicle.VehicleModel.RentalPricing;
            var token = await _protrackService.LoginVehicleAsync(vehicle);
            if (token == null)
                return ResultResponse<VehicleTrackingResponse>.Failure("Failed to retrieve tracking token from Protrack.");
            var response = new VehicleTrackingResponse
            {
                Id = vehicle.Id,
                LastMaintenanceDate = vehicle.LastMaintenanceDate,
                Description = vehicle.Description,
                BatteryHealthPercentage = vehicle.BatteryHealthPercentage,
                Color = vehicle.Color,
                CurrentOdometerKm = vehicle.CurrentOdometerKm,
                LicensePlate = vehicle.LicensePlate,
                NextMaintenanceDue = vehicle.NextMaintenanceDue,
                PurchaseDate = vehicle.PurchaseDate,
                Status = vehicle.Status,
                YearOfManufacture = vehicle.YearOfManufacture,
                rentalPricing = new RentalPricingResponse
                {
                    Id = rentalPricing.Id,
                    ExcessKmPrice = rentalPricing.ExcessKmPrice,
                    RentalPrice = rentalPricing.RentalPrice,
                },
                protrackResponse=token

            };
            return ResultResponse<VehicleTrackingResponse>.SuccessResult("Tracking token found",response);

        }
        catch (Exception ex)
        {
            return ResultResponse<VehicleTrackingResponse>.Failure($"An error occurred while retrieving vehicles: {ex.Message}");

        }
    }

    //RENTAL PRICING
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
        catch (Exception ex)
        {
            return ResultResponse<RentalPricingResponse>.Failure($"An error occurred while registering the user: {ex.Message}");

        }

    }
    public async Task<ResultResponse<List<RentalPricingResponse>>> GetAllRentalPricing()
    {
        try
        {
             var listrentalPricing=await _unitOfWork.GetRentalPricingRepository().GetAllAsync();
            var rentalPricingResponse = _mapper.Map<List<RentalPricingResponse>>(listrentalPricing);
            return ResultResponse<List<RentalPricingResponse>>.SuccessResult("RentalPricing created", rentalPricingResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<RentalPricingResponse>>.Failure($"An error occurred while registering the user: {ex.Message}");

        }
    }
}
