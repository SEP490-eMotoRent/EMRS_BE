using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.AdditionalFeeDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EMRS.Application.Services
{
    public class AdditionalFeeService : IAdditionalFeeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdditionalFeeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =====================================================================
        // 1. ADD LATE RETURN FEE (Tự động tính)
        // =====================================================================
        public async Task<ResultResponse<AdditionalFeeResponse>> AddLateReturnFeeAsync(
            AddLateReturnFeeRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1. Lấy booking
                var booking = await _unitOfWork.GetBookingRepository()
                    .GetBookingForSettlementAsync(request.BookingId);

                if (booking == null)
                    return ResultResponse<AdditionalFeeResponse>.NotFound("Booking not found");

                // 2. Kiểm tra ActualReturnDatetime đã set chưa
                if (booking.ActualReturnDatetime == null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "ActualReturnDatetime not set. Please create return receipt first.");

                // 3. Kiểm tra đã có Late Return Fee chưa
                var existingFee = booking.AdditionalFees?
                    .FirstOrDefault(f => f.FeeType == AdditionalFeeTypeEnum.LATE_RETURN.ToString());

                if (existingFee != null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "Late return fee already exists for this booking");

                // 4. Tính số giờ trễ
                var lateHours = (booking.ActualReturnDatetime.Value - booking.EndDatetime.Value).TotalHours;

                if (lateHours <= 0)
                    return ResultResponse<AdditionalFeeResponse>.SuccessResult(
                        "No late return. Vehicle returned on time.", null);

                // 5. Lấy config phí trễ
                var config = await _unitOfWork.GetConfigurationRepository()
                    .Query()
                    .FirstOrDefaultAsync(c =>
                        c.Type == (int)ConfigurationTypeEnum.AdditionalFee &&
                        c.Title.StartsWith("LATE_RETURN|"));

                if (config == null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "Late return fee configuration not found");

                var pricePerHour = decimal.Parse(config.Value);

                // 6. Tính phí
                var totalFee = (decimal)Math.Ceiling(lateHours) * pricePerHour;

                // 7. Tạo AdditionalFee
                var additionalFee = new AdditionalFee
                {
                    BookingId = request.BookingId,
                    FeeType = AdditionalFeeTypeEnum.LATE_RETURN.ToString(),
                    Description = $"Trả xe trễ {Math.Ceiling(lateHours)} giờ × {pricePerHour:N0}đ/giờ",
                    Amount = totalFee
                };

                await _unitOfWork.GetAdditionalFeeRepository().AddAsync(additionalFee);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                var response = new AdditionalFeeResponse
                {
                    Id = additionalFee.Id,
                    BookingId = additionalFee.BookingId,
                    FeeType = additionalFee.FeeType,
                    Description = additionalFee.Description,
                    Amount = additionalFee.Amount,
                    CreatedAt = additionalFee.CreatedAt
                };

                return ResultResponse<AdditionalFeeResponse>.SuccessResult(
                    "Late return fee added successfully", response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<AdditionalFeeResponse>.Failure($"Error: {ex.Message}");
            }
        }

        // =====================================================================
        // 2. ADD CLEANING FEE (Cố định 50k)
        // =====================================================================
        public async Task<ResultResponse<AdditionalFeeResponse>> AddCleaningFeeAsync(
            AddCleaningFeeRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1. Lấy booking
                var booking = await _unitOfWork.GetBookingRepository()
                    .FindByIdAsync(request.BookingId);

                if (booking == null)
                    return ResultResponse<AdditionalFeeResponse>.NotFound("Booking not found");

                // 2. Kiểm tra đã có Cleaning Fee chưa
                var existingFee = await _unitOfWork.GetAdditionalFeeRepository()
                    .Query()
                    .FirstOrDefaultAsync(f =>
                        f.BookingId == request.BookingId &&
                        f.FeeType == AdditionalFeeTypeEnum.CLEANING.ToString());

                if (existingFee != null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "Cleaning fee already exists for this booking");

                // 3. Lấy config
                var config = await _unitOfWork.GetConfigurationRepository()
                    .Query()
                    .FirstOrDefaultAsync(c =>
                        c.Type == (int)ConfigurationTypeEnum.AdditionalFee &&
                        c.Title.StartsWith("CLEANING|"));

                if (config == null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "Cleaning fee configuration not found");

                var cleaningFee = decimal.Parse(config.Value);

                // 4. Tạo AdditionalFee
                var additionalFee = new AdditionalFee
                {
                    BookingId = request.BookingId,
                    FeeType = AdditionalFeeTypeEnum.CLEANING.ToString(),
                    Description = "Phí vệ sinh xe",
                    Amount = cleaningFee
                };

                await _unitOfWork.GetAdditionalFeeRepository().AddAsync(additionalFee);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                var response = new AdditionalFeeResponse
                {
                    Id = additionalFee.Id,
                    BookingId = additionalFee.BookingId,
                    FeeType = additionalFee.FeeType,
                    Description = additionalFee.Description,
                    Amount = additionalFee.Amount,
                    CreatedAt = additionalFee.CreatedAt
                };

                return ResultResponse<AdditionalFeeResponse>.SuccessResult(
                    "Cleaning fee added successfully", response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<AdditionalFeeResponse>.Failure($"Error: {ex.Message}");
            }
        }

        // =====================================================================
        // 3. ADD CROSS BRANCH FEE (Tự động tính)
        // =====================================================================
        public async Task<ResultResponse<AdditionalFeeResponse>> AddCrossBranchFeeAsync(
            AddCrossBranchFeeRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1. Lấy booking
                var booking = await _unitOfWork.GetBookingRepository()
                    .Query()
                    .Include(b => b.HandoverBranch)
                    .Include(b => b.ReturnBranch)
                    .FirstOrDefaultAsync(b => b.Id == request.BookingId);

                if (booking == null)
                    return ResultResponse<AdditionalFeeResponse>.NotFound("Booking not found");

                // 2. Kiểm tra ReturnBranchId đã set chưa
                if (booking.ReturnBranchId == null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "ReturnBranchId not set. Please create return receipt first.");

                // 3. Kiểm tra đã có Cross Branch Fee chưa
                var existingFee = await _unitOfWork.GetAdditionalFeeRepository()
                    .Query()
                    .FirstOrDefaultAsync(f =>
                        f.BookingId == request.BookingId &&
                        f.FeeType == AdditionalFeeTypeEnum.CROSS_BRANCH.ToString());

                if (existingFee != null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "Cross branch fee already exists for this booking");

                // 4. Kiểm tra có trả khác chi nhánh không
                if (booking.HandoverBranchId == booking.ReturnBranchId)
                    return ResultResponse<AdditionalFeeResponse>.SuccessResult(
                        "Same branch return. No cross branch fee.", null);

                // 5. Lấy config
                var config = await _unitOfWork.GetConfigurationRepository()
                    .Query()
                    .FirstOrDefaultAsync(c =>
                        c.Type == (int)ConfigurationTypeEnum.AdditionalFee &&
                        c.Title.StartsWith("CROSS_BRANCH|"));

                if (config == null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "Cross branch fee configuration not found");

                var crossBranchFee = decimal.Parse(config.Value);

                // 6. Tạo AdditionalFee
                var additionalFee = new AdditionalFee
                {
                    BookingId = request.BookingId,
                    FeeType = AdditionalFeeTypeEnum.CROSS_BRANCH.ToString(),
                    Description = $"Trả xe khác chi nhánh ({booking.HandoverBranch?.BranchName} → {booking.ReturnBranch?.BranchName})",
                    Amount = crossBranchFee
                };

                await _unitOfWork.GetAdditionalFeeRepository().AddAsync(additionalFee);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                var response = new AdditionalFeeResponse
                {
                    Id = additionalFee.Id,
                    BookingId = additionalFee.BookingId,
                    FeeType = additionalFee.FeeType,
                    Description = additionalFee.Description,
                    Amount = additionalFee.Amount,
                    CreatedAt = additionalFee.CreatedAt
                };

                return ResultResponse<AdditionalFeeResponse>.SuccessResult(
                    "Cross branch fee added successfully", response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<AdditionalFeeResponse>.Failure($"Error: {ex.Message}");
            }
        }

        // =====================================================================
        // 4. ADD EXCESS KM FEE (Tự động tính)
        // =====================================================================
        public async Task<ResultResponse<AdditionalFeeResponse>> AddExcessKmFeeAsync(
            AddExcessKmFeeRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1. Lấy booking
                var booking = await _unitOfWork.GetBookingRepository()
                    .GetBookingForSettlementAsync(request.BookingId);

                if (booking == null)
                    return ResultResponse<AdditionalFeeResponse>.NotFound("Booking not found");

                // 2. Kiểm tra đã có Excess KM Fee chưa
                var existingFee = booking.AdditionalFees?
                    .FirstOrDefault(f => f.FeeType == AdditionalFeeTypeEnum.EXCCESS_KM.ToString());

                if (existingFee != null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "Excess km fee already exists for this booking");

                // 3. Lấy rental receipt
                var rentalReceipt = booking.RentalReceipts
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefault();

                if (rentalReceipt == null)
                    return ResultResponse<AdditionalFeeResponse>.NotFound("Rental receipt not found");

                // 4. Xác định category xe
                var vehicleCategory = booking.Vehicle.VehicleModel.Category;

                // 5. Lấy configs theo category
                var configs = await _unitOfWork.GetConfigurationRepository()
                    .Query()
                    .Where(c => c.Type == (int)ConfigurationTypeEnum.AdditionalFee &&
                                c.Title.StartsWith("EXCESS_KM|") &&
                                c.Title.Contains(vehicleCategory))
                    .ToListAsync();

                var kmLimitConfig = configs.FirstOrDefault(c => c.Title.Contains("Giới hạn km/ngày"));
                var pricePerKmConfig = configs.FirstOrDefault(c => c.Title.Contains("Phí vượt km"));

                if (kmLimitConfig == null || pricePerKmConfig == null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        $"Config not found for vehicle category: {vehicleCategory}");

                var baseKmPerDay = decimal.Parse(kmLimitConfig.Value);
                var pricePerKm = decimal.Parse(pricePerKmConfig.Value);

                // 6. Tính số ngày thuê (làm tròn lên)
                var rentalDays = Math.Ceiling((booking.EndDatetime - booking.StartDatetime).Value.TotalDays);

                // 7. Tính km limit
                var kmLimit = baseKmPerDay * (decimal)rentalDays;

                // 8. Tính km thực tế
                var actualKm = rentalReceipt.EndOdometerKm - rentalReceipt.StartOdometerKm;

                // 9. Tính km vượt
                var excessKm = Math.Max(0, actualKm - kmLimit);

                if (excessKm == 0)
                    return ResultResponse<AdditionalFeeResponse>.SuccessResult(
                        "No excess km. Within limit.", null);

                // 10. Tính phí
                var totalFee = excessKm * pricePerKm;

                // 11. Tạo AdditionalFee
                var additionalFee = new AdditionalFee
                {
                    BookingId = request.BookingId,
                    FeeType = AdditionalFeeTypeEnum.EXCCESS_KM.ToString(),
                    Description = $"Vượt {excessKm:N1} km (Giới hạn: {kmLimit:N1} km, Thực tế: {actualKm:N1} km) × {pricePerKm:N0}đ/km",
                    Amount = totalFee
                };

                await _unitOfWork.GetAdditionalFeeRepository().AddAsync(additionalFee);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                var response = new AdditionalFeeResponse
                {
                    Id = additionalFee.Id,
                    BookingId = additionalFee.BookingId,
                    FeeType = additionalFee.FeeType,
                    Description = additionalFee.Description,
                    Amount = additionalFee.Amount,
                    CreatedAt = additionalFee.CreatedAt
                };

                return ResultResponse<AdditionalFeeResponse>.SuccessResult(
                    "Excess km fee added successfully", response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<AdditionalFeeResponse>.Failure($"Error: {ex.Message}");
            }
        }

        // =====================================================================
        // 5. ADD DAMAGE FEE (Staff chọn loại và nhập số tiền)
        // =====================================================================
        public async Task<ResultResponse<AdditionalFeeResponse>> AddDamageFeeAsync(
            AddDamageFeeRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1. Lấy booking
                var booking = await _unitOfWork.GetBookingRepository()
                    .FindByIdAsync(request.BookingId);

                if (booking == null)
                    return ResultResponse<AdditionalFeeResponse>.NotFound("Booking not found");

                // 2. Lấy config của damage type này
                var config = await _unitOfWork.GetConfigurationRepository()
                    .Query()
                    .FirstOrDefaultAsync(c =>
                        c.Type == (int)ConfigurationTypeEnum.AdditionalFee &&
                        c.Title == $"DAMAGE|{request.DamageType}");

                if (config == null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        $"Damage type '{request.DamageType}' not found in configuration");

                // 3. Parse min/max từ JSON
                var priceRange = JsonSerializer.Deserialize<Dictionary<string, decimal>>(config.Value);
                var minAmount = priceRange["min"];
                var maxAmount = priceRange["max"];

                // 4. Validate số tiền Staff nhập
                if (request.Amount < minAmount || request.Amount > maxAmount)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        $"Amount must be between {minAmount:N0}đ and {maxAmount:N0}đ");

                // 5. Tạo AdditionalFee
                var description = $"{request.DamageType}: {request.Amount:N0}đ";
                if (!string.IsNullOrEmpty(request.AdditionalNotes))
                    description += $" - {request.AdditionalNotes}";

                var additionalFee = new AdditionalFee
                {
                    BookingId = request.BookingId,
                    FeeType = AdditionalFeeTypeEnum.DAMAGE.ToString(),
                    Description = description,
                    Amount = request.Amount
                };

                await _unitOfWork.GetAdditionalFeeRepository().AddAsync(additionalFee);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                var response = new AdditionalFeeResponse
                {
                    Id = additionalFee.Id,
                    BookingId = additionalFee.BookingId,
                    FeeType = additionalFee.FeeType,
                    Description = additionalFee.Description,
                    Amount = additionalFee.Amount,
                    CreatedAt = additionalFee.CreatedAt
                };

                return ResultResponse<AdditionalFeeResponse>.SuccessResult(
                    "Damage fee added successfully", response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<AdditionalFeeResponse>.Failure($"Error: {ex.Message}");
            }
        }

        // =====================================================================
        // 6. GET DAMAGE TYPES (Cho dropdown)
        // =====================================================================
        public async Task<ResultResponse<GetDamageTypesResponse>> GetDamageTypesAsync()
        {
            try
            {
                // Lấy tất cả configs DAMAGE
                var configs = await _unitOfWork.GetConfigurationRepository()
                    .Query()
                    .Where(c => c.Type == (int)ConfigurationTypeEnum.AdditionalFee &&
                                c.Title.StartsWith("DAMAGE|"))
                    .ToListAsync();

                var options = new List<DamageTypeOption>();

                foreach (var config in configs)
                {
                    // Parse title: "DAMAGE|Gương vỡ/mất"
                    var damageType = config.Title.Split('|')[1];

                    // Parse JSON: {"min": 200000, "max": 400000}
                    var priceRange = JsonSerializer.Deserialize<Dictionary<string, decimal>>(config.Value);
                    var minAmount = priceRange["min"];
                    var maxAmount = priceRange["max"];

                    options.Add(new DamageTypeOption
                    {
                        DamageType = damageType,
                        Description = config.Description,
                        MinAmount = minAmount,
                        MaxAmount = maxAmount,
                        DisplayText = $"{damageType} ({minAmount:N0}đ - {maxAmount:N0}đ)"
                    });
                }

                var response = new GetDamageTypesResponse { Options = options };

                return ResultResponse<GetDamageTypesResponse>.SuccessResult(
                    "Damage types retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<GetDamageTypesResponse>.Failure($"Error: {ex.Message}");
            }
        }

        // =====================================================================
        // 7. GET FEES BY BOOKING ID
        // =====================================================================
        public async Task<ResultResponse<List<AdditionalFeeResponse>>> GetFeesByBookingIdAsync(
            Guid bookingId)
        {
            try
            {
                var fees = await _unitOfWork.GetAdditionalFeeRepository()
                    .GetAdditionalFeesByBookingIdAsync(bookingId);

                var response = fees.Select(f => new AdditionalFeeResponse
                {
                    Id = f.Id,
                    BookingId = f.BookingId,
                    FeeType = f.FeeType,
                    Description = f.Description,
                    Amount = f.Amount,
                    CreatedAt = f.CreatedAt
                }).ToList();

                return ResultResponse<List<AdditionalFeeResponse>>.SuccessResult(
                    "Fees retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<AdditionalFeeResponse>>.Failure($"Error: {ex.Message}");
            }
        }

        // =====================================================================
        // 8. DELETE FEE
        // =====================================================================
        public async Task<ResultResponse<string>> DeleteFeeAsync(Guid feeId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var fee = await _unitOfWork.GetAdditionalFeeRepository().FindByIdAsync(feeId);

                if (fee == null)
                    return ResultResponse<string>.NotFound("Fee not found");

                _unitOfWork.GetAdditionalFeeRepository().Delete(fee);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return ResultResponse<string>.SuccessResult("Fee deleted successfully", null);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<string>.Failure($"Error: {ex.Message}");
            }
        }
    }

}
