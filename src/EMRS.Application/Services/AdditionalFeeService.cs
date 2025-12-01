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

        public async Task<ResultResponse<AdditionalFeeResponse>> AddLateReturnFeeAsync(
            AddLateReturnFeeRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var booking = await _unitOfWork.GetBookingRepository()
                    .GetBookingForSettlementAsync(request.BookingId);

                if (booking == null)
                    return ResultResponse<AdditionalFeeResponse>.NotFound("Booking not found");

                if (booking.ActualReturnDatetime == null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "ActualReturnDatetime not set. Please create return receipt first.");

                var existingFee = booking.AdditionalFees?
                    .FirstOrDefault(f => f.FeeType == AdditionalFeeTypeEnum.LATE_RETURN.ToString());

                if (existingFee != null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "Late return fee already exists for this booking");

                var lateHours = (booking.ActualReturnDatetime.Value - booking.EndDatetime.Value).TotalHours;

                if (lateHours <= 0)
                    return ResultResponse<AdditionalFeeResponse>.SuccessResult(
                        "No late return. Vehicle returned on time.", null);

                
                var config = await _unitOfWork.GetConfigurationRepository()
                    .Query()
                    .FirstOrDefaultAsync(c =>
                        c.Type == (int)ConfigurationTypeEnum.AdditionalFee &&
                        c.Title == "Phí trả xe trễ");

                if (config == null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "Late return fee configuration not found");

                var pricePerHour = decimal.Parse(config.Value);
                var totalFee = (decimal)Math.Ceiling(lateHours) * pricePerHour;

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

        
        public async Task<ResultResponse<AdditionalFeeResponse>> AddCleaningFeeAsync(
            AddCleaningFeeRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var booking = await _unitOfWork.GetBookingRepository()
                    .FindByIdAsync(request.BookingId);

                if (booking == null)
                    return ResultResponse<AdditionalFeeResponse>.NotFound("Booking not found");

                var existingFee = await _unitOfWork.GetAdditionalFeeRepository()
                    .Query()
                    .FirstOrDefaultAsync(f =>
                        f.BookingId == request.BookingId &&
                        f.FeeType == AdditionalFeeTypeEnum.CLEANING.ToString());

                if (existingFee != null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "Cleaning fee already exists for this booking");

                
                var config = await _unitOfWork.GetConfigurationRepository()
                    .Query()
                    .FirstOrDefaultAsync(c =>
                        c.Type == (int)ConfigurationTypeEnum.AdditionalFee &&
                        c.Title == "Phí vệ sinh xe");

                if (config == null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "Cleaning fee configuration not found");

                var cleaningFee = decimal.Parse(config.Value);

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

        
        public async Task<ResultResponse<AdditionalFeeResponse>> AddCrossBranchFeeAsync(
            AddCrossBranchFeeRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var booking = await _unitOfWork.GetBookingRepository()
                    .Query()
                    .Include(b => b.HandoverBranch)
                    .Include(b => b.ReturnBranch)
                    .FirstOrDefaultAsync(b => b.Id == request.BookingId);

                if (booking == null)
                    return ResultResponse<AdditionalFeeResponse>.NotFound("Booking not found");

                if (booking.ReturnBranchId == null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "ReturnBranchId not set. Please create return receipt first.");

                var existingFee = await _unitOfWork.GetAdditionalFeeRepository()
                    .Query()
                    .FirstOrDefaultAsync(f =>
                        f.BookingId == request.BookingId &&
                        f.FeeType == AdditionalFeeTypeEnum.CROSS_BRANCH.ToString());

                if (existingFee != null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "Cross branch fee already exists for this booking");

                if (booking.HandoverBranchId == booking.ReturnBranchId)
                    return ResultResponse<AdditionalFeeResponse>.SuccessResult(
                        "Same branch return. No cross branch fee.", null);

               
                var config = await _unitOfWork.GetConfigurationRepository()
                    .Query()
                    .FirstOrDefaultAsync(c =>
                        c.Type == (int)ConfigurationTypeEnum.AdditionalFee &&
                        c.Title == "Phí trả xe khác chi nhánh");

                if (config == null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "Cross branch fee configuration not found");

                var crossBranchFee = decimal.Parse(config.Value);

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

        
        public async Task<ResultResponse<AdditionalFeeResponse>> AddExcessKmFeeAsync(
            AddExcessKmFeeRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var booking = await _unitOfWork.GetBookingRepository()
                    .GetBookingForSettlementAsync(request.BookingId);

                if (booking == null)
                    return ResultResponse<AdditionalFeeResponse>.NotFound("Booking not found");

                var existingFee = booking.AdditionalFees?
                    .FirstOrDefault(f => f.FeeType == AdditionalFeeTypeEnum.EXCCESS_KM.ToString());

                if (existingFee != null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        "Excess km fee already exists for this booking");

                var rentalReceipt = booking.RentalReceipts
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefault();

                if (rentalReceipt == null)
                    return ResultResponse<AdditionalFeeResponse>.NotFound("Rental receipt not found");

                var vehicleCategory = booking.Vehicle.VehicleModel.Category;

                
                var categoryVN = vehicleCategory switch
                {
                    "ECONOMY" => "Phổ thông",
                    "STANDARD" => "Trung cấp",
                    "PREMIUM" => "Cao cấp",
                    _ => throw new ArgumentException("Invalid category")
                };

                
                var configs = await _unitOfWork.GetConfigurationRepository()
                    .Query()
                    .Where(c => c.Type == (int)ConfigurationTypeEnum.AdditionalFee &&
                                c.Title.Contains(categoryVN))
                    .ToListAsync();

                var kmLimitConfig = configs.FirstOrDefault(c => c.Title.Contains("Giới hạn km"));
                var pricePerKmConfig = configs.FirstOrDefault(c => c.Title.Contains("Phí vượt km"));

                if (kmLimitConfig == null || pricePerKmConfig == null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        $"Config not found for vehicle category: {categoryVN}");

                var baseKmPerDay = decimal.Parse(kmLimitConfig.Value);
                var pricePerKm = decimal.Parse(pricePerKmConfig.Value);

                var rentalDays = Math.Ceiling((booking.EndDatetime - booking.StartDatetime).Value.TotalDays);
                var kmLimit = baseKmPerDay * (decimal)rentalDays;
                var actualKm = rentalReceipt.EndOdometerKm - rentalReceipt.StartOdometerKm;
                var excessKm = Math.Max(0, actualKm - kmLimit);

                if (excessKm == 0)
                    return ResultResponse<AdditionalFeeResponse>.SuccessResult(
                        "No excess km. Within limit.", null);

                var totalFee = excessKm * pricePerKm;

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

        
        public async Task<ResultResponse<AdditionalFeeResponse>> AddDamageFeeAsync(
            AddDamageFeeRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var booking = await _unitOfWork.GetBookingRepository()
                    .FindByIdAsync(request.BookingId);

                if (booking == null)
                    return ResultResponse<AdditionalFeeResponse>.NotFound("Booking not found");

                
                var configsMin = await _unitOfWork.GetConfigurationRepository()
                    .Query()
                    .Where(c => c.Type == (int)ConfigurationTypeEnum.AdditionalFee &&
                                c.Title.Contains(request.DamageType) &&
                                c.Title.Contains("(Min)"))
                    .FirstOrDefaultAsync();

                var configsMax = await _unitOfWork.GetConfigurationRepository()
                    .Query()
                    .Where(c => c.Type == (int)ConfigurationTypeEnum.AdditionalFee &&
                                c.Title.Contains(request.DamageType) &&
                                c.Title.Contains("(Max)"))
                    .FirstOrDefaultAsync();

                if (configsMin == null || configsMax == null)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        $"Damage type '{request.DamageType}' not found in configuration");

                var minAmount = decimal.Parse(configsMin.Value);
                var maxAmount = decimal.Parse(configsMax.Value);

                if (request.Amount < minAmount || request.Amount > maxAmount)
                    return ResultResponse<AdditionalFeeResponse>.Failure(
                        $"Amount must be between {minAmount:N0}đ and {maxAmount:N0}đ");

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

        
        public async Task<ResultResponse<GetDamageTypesResponse>> GetDamageTypesAsync()
        {
            try
            {
              
                var configs = await _unitOfWork.GetConfigurationRepository()
                    .Query()
                    .Where(c => c.Type == (int)ConfigurationTypeEnum.AdditionalFee &&
                                c.Title.StartsWith("Hư hỏng"))
                    .ToListAsync();

                
                var damageTypes = new Dictionary<string, (decimal min, decimal max, string desc)>();

                foreach (var config in configs)
                {
                    // Parse: "Hư hỏng TB - Gương vỡ/mất (Min)"
                    var parts = config.Title.Split(" - ");
                    if (parts.Length < 2) continue;

                    var damageType = parts[1].Replace(" (Min)", "").Replace(" (Max)", "");

                    if (!damageTypes.ContainsKey(damageType))
                    {
                        damageTypes[damageType] = (0, 0, parts[0]); // "Hư hỏng TB"
                    }

                    if (config.Title.Contains("(Min)"))
                    {
                        var current = damageTypes[damageType];
                        damageTypes[damageType] = (decimal.Parse(config.Value), current.max, current.desc);
                    }
                    else if (config.Title.Contains("(Max)"))
                    {
                        var current = damageTypes[damageType];
                        damageTypes[damageType] = (current.min, decimal.Parse(config.Value), current.desc);
                    }
                }

                var options = damageTypes.Select(kvp => new DamageTypeOption
                {
                    DamageType = kvp.Key,
                    Description = kvp.Value.desc,
                    MinAmount = kvp.Value.min,
                    MaxAmount = kvp.Value.max,
                    DisplayText = $"{kvp.Key} ({kvp.Value.min:N0}đ - {kvp.Value.max:N0}đ)"
                }).ToList();

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
