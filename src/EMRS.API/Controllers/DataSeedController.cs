using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataSeedController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;

        public DataSeedController(
            IUnitOfWork unitOfWork,
            ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        
        private static DateTimeOffset GetVietnamTime()
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, vietnamTimeZone);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateSeedData()
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var currentTime = GetVietnamTime();

                // ================== CẤU HÌNH ĐƯỜNG DẪN ẢNH ==================
                var imageBasePath = @"C:\Users\Dang\Pictures\Vehicle Image";

                var vehicleModelImages = new Dictionary<string, string>
                {
                    { "VinFast Klara S", "Vinfast_Klara_S.png" },
                    { "VinFast Evo200", "Vinfast_Evo_200.png" },
                    { "VinFast Impes", "Vinfast_Impes.png" },
                    { "VinFast Ludo", "Vinfast_Ludo.png" },
                    { "VinFast Theon S", "VinfastTheonS.png" },
                    { "Yadea G5", "Yadea_G5.jpg" },
                    { "Yadea Xmen Neo", "Yadea_Xmen_Neo.png" },
                    { "Pega Newtech", "Pega_newtech.jpg" }
                };

                // ================== 1. TẠO RENTAL PRICING ==================
                var economyPricing = new RentalPricing
                {
                    RentalPrice = 100000,
                };

                var standardPricing = new RentalPricing
                {
                    RentalPrice = 140000,
                };

                var premiumPricing = new RentalPricing
                {
                    RentalPrice = 180000,
                };

                await _unitOfWork.GetRentalPricingRepository().AddAsync(economyPricing);
                await _unitOfWork.GetRentalPricingRepository().AddAsync(standardPricing);
                await _unitOfWork.GetRentalPricingRepository().AddAsync(premiumPricing);
                await _unitOfWork.SaveChangesAsync();

                // ================== 2. TẠO 2 BRANCHES ==================
                var branch1 = new Branch
                {
                    BranchName = "Chi nhánh Quận 1",
                    Address = "123 Nguyễn Huệ, Quận 1",
                    City = "Hồ Chí Minh",
                    Phone = "0283822345",
                    Email = "quan1@emotorent.vn",
                    Latitude = 10.7769,
                    Longitude = 106.7009,
                    OpeningTime = "06:00",
                    ClosingTime = "22:00"
                };

                var branch2 = new Branch
                {
                    BranchName = "Chi nhánh Quận 3",
                    Address = "456 Võ Văn Tần, Quận 3",
                    City = "Hồ Chí Minh",
                    Phone = "0283933456",
                    Email = "quan3@emotorent.vn",
                    Latitude = 10.7826,
                    Longitude = 106.6920,
                    OpeningTime = "06:00",
                    ClosingTime = "22:00"
                };

                await _unitOfWork.GetBranchRepository().AddAsync(branch1);
                await _unitOfWork.GetBranchRepository().AddAsync(branch2);
                await _unitOfWork.SaveChangesAsync();

                // ================== 3. TẠO VEHICLE MODELS VÀ UPLOAD ẢNH ==================
                var vehicleModels = new List<VehicleModel>
                {
                    new VehicleModel
                    {
                        ModelName = "VinFast Klara S",
                        Category = VehicleCategoryEnum.ECONOMY.ToString(),
                        BatteryCapacityKwh = 1.2m,
                        MaxRangeKm = 80,
                        MaxSpeedKmh = 50,
                        Description = "Xe máy điện phổ thông cho sinh viên và công sở",
                        RentalPricingId = economyPricing.Id
                    },
                    new VehicleModel
                    {
                        ModelName = "VinFast Evo200",
                        Category = VehicleCategoryEnum.ECONOMY.ToString(),
                        BatteryCapacityKwh = 1.5m,
                        MaxRangeKm = 90,
                        MaxSpeedKmh = 55,
                        Description = "Xe máy điện thông minh với thiết kế trẻ trung",
                        RentalPricingId = economyPricing.Id
                    },
                    new VehicleModel
                    {
                        ModelName = "VinFast Impes",
                        Category = VehicleCategoryEnum.STANDARD.ToString(),
                        BatteryCapacityKwh = 2.0m,
                        MaxRangeKm = 120,
                        MaxSpeedKmh = 90,
                        Description = "Xe máy điện cao cấp với hiệu suất mạnh mẽ",
                        RentalPricingId = standardPricing.Id
                    },
                    new VehicleModel
                    {
                        ModelName = "VinFast Ludo",
                        Category = VehicleCategoryEnum.ECONOMY.ToString(),
                        BatteryCapacityKwh = 1.0m,
                        MaxRangeKm = 70,
                        MaxSpeedKmh = 45,
                        Description = "Xe máy điện nhỏ gọn tiện lợi di chuyển đô thị",
                        RentalPricingId = economyPricing.Id
                    },
                    new VehicleModel
                    {
                        ModelName = "VinFast Theon S",
                        Category = VehicleCategoryEnum.PREMIUM.ToString(),
                        BatteryCapacityKwh = 2.5m,
                        MaxRangeKm = 150,
                        MaxSpeedKmh = 99,
                        Description = "Xe máy điện cao cấp nhất với công nghệ thông minh",
                        RentalPricingId = premiumPricing.Id
                    },
                    new VehicleModel
                    {
                        ModelName = "Yadea G5",
                        Category = VehicleCategoryEnum.STANDARD.ToString(),
                        BatteryCapacityKwh = 1.8m,
                        MaxRangeKm = 100,
                        MaxSpeedKmh = 65,
                        Description = "Xe máy điện Trung Quốc chất lượng tốt",
                        RentalPricingId = standardPricing.Id
                    },
                    new VehicleModel
                    {
                        ModelName = "Yadea Xmen Neo",
                        Category = VehicleCategoryEnum.STANDARD.ToString(),
                        BatteryCapacityKwh = 2.2m,
                        MaxRangeKm = 110,
                        MaxSpeedKmh = 75,
                        Description = "Xe máy điện thiết kế thể thao năng động",
                        RentalPricingId = standardPricing.Id
                    },
                    new VehicleModel
                    {
                        ModelName = "Pega Newtech",
                        Category = VehicleCategoryEnum.ECONOMY.ToString(),
                        BatteryCapacityKwh = 1.3m,
                        MaxRangeKm = 85,
                        MaxSpeedKmh = 50,
                        Description = "Xe máy điện Việt Nam giá rẻ bền bỉ",
                        RentalPricingId = economyPricing.Id
                    }
                };

                // Upload ảnh cho VehicleModel
                var mediaList = new List<Media>();

                foreach (var model in vehicleModels)
                {
                    await _unitOfWork.GetVehicleModelRepository().AddAsync(model);
                    await _unitOfWork.SaveChangesAsync();

                    if (vehicleModelImages.TryGetValue(model.ModelName, out var imageName))
                    {
                        var imagePath = Path.Combine(imageBasePath, imageName);

                        if (System.IO.File.Exists(imagePath))
                        {
                            using (var stream = System.IO.File.OpenRead(imagePath))
                            {
                                var formFile = new FormFile(stream, 0, stream.Length, "file", imageName)
                                {
                                    Headers = new HeaderDictionary(),
                                    ContentType = "image/jpeg"
                                };

                                var imageUrl = await _cloudinaryService.UploadImageFileAsync(
                                    formFile,
                                    $"model_{model.ModelName.Replace(" ", "_")}_{currentTime:yyyyMMddHHmmss}",
                                    "VehicleModel"
                                );

                                if (!string.IsNullOrEmpty(imageUrl))
                                {
                                    mediaList.Add(new Media
                                    {
                                        EntityType = MediaEntityTypeEnum.VehicleModel.ToString(),
                                        FileUrl = imageUrl,
                                        DocNo = model.Id,
                                        MediaType = MediaTypeEnum.Image.ToString()
                                    });
                                }
                            }
                        }
                    }
                }

                await _unitOfWork.GetMediaRepository().AddRangeAsync(mediaList);
                await _unitOfWork.SaveChangesAsync();

                // ================== 4. TẠO 20 VEHICLES (LUÔN CÓ 2 KLARA S) ==================
                var vehicles = new List<Vehicle>();
                var colors = new[] { "Đỏ", "Trắng", "Xanh dương", "Đen", "Xám bạc" };
                var random = new Random();

                // Lấy model Klara S
                var klaraSModel = vehicleModels.First(vm => vm.ModelName == "VinFast Klara S");

                // Lấy các model khác (không phải Klara S) để random
                var otherModels = vehicleModels.Where(vm => vm.ModelName != "VinFast Klara S").ToList();

                // ========== CHI NHÁNH 1: 1 Klara S + 9 xe random ==========

                // Xe 1: VinFast Klara S (cố định) - Có GPS tracking
                vehicles.Add(new Vehicle
                {
                    LicensePlate = "43-LD123.45",
                    Color = colors[random.Next(colors.Length)],
                    YearOfManufacture = currentTime.AddYears(-1).UtcDateTime,
                    CurrentOdometerKm = random.Next(100, 500),
                    BatteryHealthPercentage = random.Next(90, 100),
                    Status = VehicleStatusEnum.Available.ToString(),
                    PurchaseDate = currentTime.AddYears(-1).UtcDateTime,
                    Description = "Xe VinFast Klara S tình trạng tốt",
                    VehicleModelId = klaraSModel.Id,
                    BranchId = branch1.Id,
                    GpsDeviceIdent = "355468592699953",  // ✅ GPS Device ID
                    FlespiDeviceId = 7263099              // ✅ Flespi Device ID
                });

                // Xe 2-10: Random từ các model khác
                for (int i = 2; i <= 10; i++)
                {
                    var model = otherModels[random.Next(otherModels.Count)];

                    vehicles.Add(new Vehicle
                    {
                        LicensePlate = $"59K1-{12340 + i}",
                        Color = colors[random.Next(colors.Length)],
                        YearOfManufacture = currentTime.AddYears(-1).UtcDateTime,
                        CurrentOdometerKm = random.Next(100, 500),
                        BatteryHealthPercentage = random.Next(90, 100),
                        Status = VehicleStatusEnum.Available.ToString(),
                        PurchaseDate = currentTime.AddYears(-1).UtcDateTime,
                        Description = $"Xe {model.ModelName} tình trạng tốt",
                        VehicleModelId = model.Id,
                        BranchId = branch1.Id
                    });
                }

                // ========== CHI NHÁNH 2: 1 Klara S + 9 xe random ==========

                // Xe 11: VinFast Klara S (cố định) - Có GPS tracking
                vehicles.Add(new Vehicle
                {
                    LicensePlate = "59K1-12351",
                    Color = colors[random.Next(colors.Length)],
                    YearOfManufacture = currentTime.AddYears(-1).UtcDateTime,
                    CurrentOdometerKm = random.Next(100, 500),
                    BatteryHealthPercentage = random.Next(90, 100),
                    Status = VehicleStatusEnum.Available.ToString(),
                    PurchaseDate = currentTime.AddYears(-1).UtcDateTime,
                    Description = "Xe VinFast Klara S tình trạng tốt",
                    VehicleModelId = klaraSModel.Id,
                    BranchId = branch2.Id,
                    GpsDeviceIdent = "355468593172349",  // ✅ GPS Device ID
                    FlespiDeviceId = 7354990              // ✅ Flespi Device ID
                });

                // Xe 12-20: Random từ các model khác
                for (int i = 12; i <= 20; i++)
                {
                    var model = otherModels[random.Next(otherModels.Count)];

                    vehicles.Add(new Vehicle
                    {
                        LicensePlate = $"59K1-{12340 + i}",
                        Color = colors[random.Next(colors.Length)],
                        YearOfManufacture = currentTime.AddYears(-1).UtcDateTime,
                        CurrentOdometerKm = random.Next(100, 500),
                        BatteryHealthPercentage = random.Next(90, 100),
                        Status = VehicleStatusEnum.Available.ToString(),
                        PurchaseDate = currentTime.AddYears(-1).UtcDateTime,
                        Description = $"Xe {model.ModelName} tình trạng tốt",
                        VehicleModelId = model.Id,
                        BranchId = branch2.Id
                    });
                }

                // Upload ảnh cho từng Vehicle
                var vehicleMediaList = new List<Media>();

                foreach (var vehicle in vehicles)
                {
                    await _unitOfWork.GetVehicleRepository().AddAsync(vehicle);
                    await _unitOfWork.SaveChangesAsync();

                    var vehicleModel = vehicleModels.First(vm => vm.Id == vehicle.VehicleModelId);

                    if (vehicleModelImages.TryGetValue(vehicleModel.ModelName, out var imageName))
                    {
                        var imagePath = Path.Combine(imageBasePath, imageName);

                        if (System.IO.File.Exists(imagePath))
                        {
                            using (var stream = System.IO.File.OpenRead(imagePath))
                            {
                                var formFile = new FormFile(stream, 0, stream.Length, "file", imageName)
                                {
                                    Headers = new HeaderDictionary(),
                                    ContentType = "image/jpeg"
                                };

                                var imageUrl = await _cloudinaryService.UploadImageFileAsync(
                                    formFile,
                                    $"vehicle_{vehicle.LicensePlate.Replace("-", "_")}_{currentTime:yyyyMMddHHmmss}",
                                    "Vehicle"
                                );

                                if (!string.IsNullOrEmpty(imageUrl))
                                {
                                    vehicleMediaList.Add(new Media
                                    {
                                        EntityType = MediaEntityTypeEnum.Vehicle.ToString(),
                                        FileUrl = imageUrl,
                                        DocNo = vehicle.Id,
                                        MediaType = MediaTypeEnum.Image.ToString()
                                    });
                                }
                            }
                        }
                    }
                }

                await _unitOfWork.GetMediaRepository().AddRangeAsync(vehicleMediaList);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();

                // Đếm số lượng Klara S để verify
                var klaraSCount = vehicles.Count(v => v.VehicleModelId == klaraSModel.Id);

                return Ok(new
                {
                    success = true,
                    message = "Seed data generated successfully",
                    vietnamTime = currentTime,
                    data = new
                    {
                        branches = 2,
                        rentalPricings = 3,
                        vehicleModels = vehicleModels.Count,
                        vehicles = vehicles.Count,
                        klaraSCount = klaraSCount, // ✅ Luôn = 2
                        mediaUploaded = mediaList.Count + vehicleMediaList.Count
                    }
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return BadRequest(new
                {
                    success = false,
                    message = $"Error generating seed data: {ex.Message}",
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}