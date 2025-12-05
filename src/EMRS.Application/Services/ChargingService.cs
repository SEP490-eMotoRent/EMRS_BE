// EMRS.Application/Services/ChargingService.cs
using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.ChargingRecordDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class ChargingService : IChargingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public ChargingService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ResultResponse<BookingChargingSearchResponse>> SearchBookingByLicensePlate(string licensePlate)
    {
        try
        {
            
            var vehicle = await _unitOfWork.GetVehicleRepository()
                .Query()
                .Include(v => v.VehicleModel)
                .Include(v => v.Branch)
                .Where(v => v.LicensePlate == licensePlate)
                .FirstOrDefaultAsync();

            if (vehicle == null)
                return ResultResponse<BookingChargingSearchResponse>.NotFound($"Không tìm thấy xe có biển số {licensePlate}");

            
            var booking = await _unitOfWork.GetBookingRepository()
                .Query()
                .Include(b => b.Renter)
                    .ThenInclude(r => r.Account)
                .Include(b => b.VehicleModel)
                .Include(b => b.RentalReceipts)
                .Where(b => b.VehicleId == vehicle.Id && b.BookingStatus == BookingStatusEnum.Renting.ToString())
                .FirstOrDefaultAsync();

            if (booking == null)
                return ResultResponse<BookingChargingSearchResponse>.NotFound($"Xe {licensePlate} hiện không có booking nào đang thuê");

            
            var lastChargingRecord = await _unitOfWork.GetChargingRecordRepository()
                .GetLastChargingRecordByBookingIdAsync(booking.Id);

            
            var handoverReceipt = booking.RentalReceipts
                .OrderBy(rr => rr.CreatedAt)
                .FirstOrDefault();

            var response = new BookingChargingSearchResponse
            {
                BookingId = booking.Id,
                BookingCode = booking.BookingCode,
                BookingStatus = booking.BookingStatus,
                RenterFullName = booking.Renter.Account.Fullname ?? "N/A",
                VehicleModelName = booking.VehicleModel.ModelName,
                LicensePlate = vehicle.LicensePlate,
                BranchAddress = vehicle.Branch.Address,
                BatteryAtHandover = handoverReceipt?.StartBatteryPercentage ?? 0,
                LastChargingDate = lastChargingRecord?.ChargingDate
            };

            return ResultResponse<BookingChargingSearchResponse>.SuccessResult(
                "Tìm thấy booking đang thuê xe", response);
        }
        catch (Exception ex)
        {
            return ResultResponse<BookingChargingSearchResponse>.Failure($"Lỗi khi tìm kiếm: {ex.Message}");
        }
    }

    public async Task<ResultResponse<ChargingRateResponse>> GetChargingRate(ChargingRateRequest request)
    {
        try
        {
            var (timeSlot, rate, description) = await DetermineChargingRateAsync(request.ChargingDate);

            var friendlyDescription = FormatChargingDescription(description, request.ChargingDate);

            var response = new ChargingRateResponse
            {
                ChargingDate = request.ChargingDate,
                TimeSlot = timeSlot,
                RatePerKwh = rate,
                Description = friendlyDescription
            };

            return ResultResponse<ChargingRateResponse>.SuccessResult(
                "Lấy bảng giá sạc thành công", response);
        }
        catch (Exception ex)
        {
            return ResultResponse<ChargingRateResponse>.Failure($"Lỗi khi lấy bảng giá: {ex.Message}");
        }
    }

    public async Task<ResultResponse<ChargingRecordResponse>> CreateChargingRecord(ChargingRecordCreateRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

           
            var booking = await _unitOfWork.GetBookingRepository().FindByIdAsync(request.BookingId);
            if (booking == null)
                return ResultResponse<ChargingRecordResponse>.NotFound("Không tìm thấy booking");

            if (booking.BookingStatus != BookingStatusEnum.Renting.ToString())
                return ResultResponse<ChargingRecordResponse>.Failure("Booking không ở trạng thái đang thuê");

            
            if (request.StartBatteryPercentage >= request.EndBatteryPercentage)
                return ResultResponse<ChargingRecordResponse>.Failure("% pin sau sạc phải lớn hơn % pin trước sạc");

            if (request.KwhCharged <= 0)
                return ResultResponse<ChargingRecordResponse>.Failure("Số kWh sạc phải lớn hơn 0");

            
            var staffId = Guid.Parse(_currentUserService.UserId);
            var staff = await _unitOfWork.GetStaffRepository().FindByIdAsync(staffId);
            if (staff == null)
                return ResultResponse<ChargingRecordResponse>.Failure("Không tìm thấy thông tin nhân viên");

            
            var (timeSlot, ratePerKwh, description) = await DetermineChargingRateAsync(request.ChargingDate);

            
            var fee = request.KwhCharged * ratePerKwh;
            var batteryPercentageCharged = request.EndBatteryPercentage - request.StartBatteryPercentage;

            
            var chargingRecord = new ChargingRecord
            {
                ChargingDate = request.ChargingDate,
                StartBatteryPercentage = request.StartBatteryPercentage,
                EndBatteryPercentage = request.EndBatteryPercentage,
                KwhCharged = request.KwhCharged,
                RatePerKwh = ratePerKwh,
                Fee = fee,
                Notes = request.Notes,
                BookingId = request.BookingId,
                BranchId = (Guid)staff.BranchId,
                StaffId = staffId
            };

            await _unitOfWork.GetChargingRecordRepository().AddAsync(chargingRecord);

            
            booking.TotalChargingFee += fee;
            _unitOfWork.GetBookingRepository().Update(booking);

            
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            
            var response = new ChargingRecordResponse
            {
                Id = chargingRecord.Id,
                ChargingDate = chargingRecord.ChargingDate ?? DateTime.UtcNow,
                StartBatteryPercentage = chargingRecord.StartBatteryPercentage,
                EndBatteryPercentage = chargingRecord.EndBatteryPercentage,
                BatteryPercentageCharged = batteryPercentageCharged,
                KwhCharged = chargingRecord.KwhCharged,
                RatePerKwh = chargingRecord.RatePerKwh,
                Fee = chargingRecord.Fee,
                TimeSlot = timeSlot,
                Notes = chargingRecord.Notes
            };

            return ResultResponse<ChargingRecordResponse>.SuccessResult(
                "Tạo phiếu sạc thành công", response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<ChargingRecordResponse>.Failure($"Lỗi khi tạo phiếu sạc: {ex.Message}");
        }
    }

    public async Task<ResultResponse<List<ChargingRecordListResponse>>> GetChargingRecordsByRenter()
    {
        try
        {
            var renterId = Guid.Parse(_currentUserService.UserId);

            var chargingRecords = await _unitOfWork.GetChargingRecordRepository()
                .GetChargingRecordsByRenterIdAsync(renterId);

            if (!chargingRecords.Any())
                return ResultResponse<List<ChargingRecordListResponse>>.SuccessResult(
                    "Chưa có lịch sử sạc xe", new List<ChargingRecordListResponse>());

            var responseList = new List<ChargingRecordListResponse>();
            foreach (var cr in chargingRecords)
            {
                responseList.Add(new ChargingRecordListResponse
                {
                    Id = cr.Id,
                    ChargingDate = cr.ChargingDate ?? DateTime.UtcNow,
                    StartBatteryPercentage = cr.StartBatteryPercentage,
                    EndBatteryPercentage = cr.EndBatteryPercentage,
                    BatteryPercentageCharged = cr.EndBatteryPercentage - cr.StartBatteryPercentage,
                    KwhCharged = cr.KwhCharged,
                    RatePerKwh = cr.RatePerKwh,
                    Fee = cr.Fee,
                    TimeSlot = await GetTimeSlotFromRateAsync(cr.RatePerKwh),
                    Notes = cr.Notes,
                    BookingCode = cr.Booking.BookingCode,
                    VehicleModelName = cr.Booking.Vehicle?.VehicleModel?.ModelName ?? "N/A",
                    LicensePlate = cr.Booking.Vehicle?.LicensePlate ?? "N/A",
                    BranchName = cr.Branch.BranchName,
                    BranchAddress = cr.Branch.Address,
                    StaffName = cr.Staff.Account.Fullname ?? "N/A"
                });
            }

            return ResultResponse<List<ChargingRecordListResponse>>.SuccessResult(
                $"Lấy danh sách {responseList.Count} phiếu sạc thành công", responseList);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<ChargingRecordListResponse>>.Failure(
                $"Lỗi khi lấy danh sách phiếu sạc: {ex.Message}");
        }
    }

    public async Task<ResultResponse<List<ChargingRecordListResponse>>> GetChargingRecordsByBookingId(Guid bookingId)
    {
        try
        {
            var booking = await _unitOfWork.GetBookingRepository()
                .Query()
                .Include(b => b.Vehicle)
                    .ThenInclude(v => v.VehicleModel)
                .Where(b => b.Id == bookingId)
                .FirstOrDefaultAsync();

            if (booking == null)
                return ResultResponse<List<ChargingRecordListResponse>>.NotFound("Không tìm thấy booking");

            var chargingRecords = await _unitOfWork.GetChargingRecordRepository()
                .Query()
                .Include(cr => cr.Booking)
                .Include(cr => cr.Branch)
                .Include(cr => cr.Staff)
                    .ThenInclude(s => s.Account)
                .Where(cr => cr.BookingId == bookingId)
                .OrderByDescending(cr => cr.ChargingDate)
                .ToListAsync();

            if (!chargingRecords.Any())
                return ResultResponse<List<ChargingRecordListResponse>>.SuccessResult(
                    "Booking này chưa có lịch sử sạc xe", new List<ChargingRecordListResponse>());

            var responseList = new List<ChargingRecordListResponse>();
            foreach (var cr in chargingRecords)
            {
                responseList.Add(new ChargingRecordListResponse
                {
                    Id = cr.Id,
                    ChargingDate = cr.ChargingDate ?? DateTime.UtcNow,
                    StartBatteryPercentage = cr.StartBatteryPercentage,
                    EndBatteryPercentage = cr.EndBatteryPercentage,
                    BatteryPercentageCharged = cr.EndBatteryPercentage - cr.StartBatteryPercentage,
                    KwhCharged = cr.KwhCharged,
                    RatePerKwh = cr.RatePerKwh,
                    Fee = cr.Fee,
                    TimeSlot = await GetTimeSlotFromRateAsync(cr.RatePerKwh),
                    Notes = cr.Notes,
                    BookingCode = booking.BookingCode,
                    VehicleModelName = booking.Vehicle?.VehicleModel?.ModelName ?? "N/A",
                    LicensePlate = booking.Vehicle?.LicensePlate ?? "N/A",
                    BranchName = cr.Branch.BranchName,
                    BranchAddress = cr.Branch.Address,
                    StaffName = cr.Staff.Account.Fullname ?? "N/A"
                });
            }

            return ResultResponse<List<ChargingRecordListResponse>>.SuccessResult(
                $"Lấy danh sách {responseList.Count} phiếu sạc thành công", responseList);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<ChargingRecordListResponse>>.Failure(
                $"Lỗi khi lấy danh sách phiếu sạc: {ex.Message}");
        }
    }

    #region Private Helper Methods

    private async Task<(string TimeSlot, decimal Rate, string Description)> DetermineChargingRateAsync(DateTime chargingDate)
    {
        
        var chargingConfigs = await _unitOfWork.GetConfigurationRepository()
            .Query()
            .Where(c => (c.Type == (int)ConfigurationTypeEnum.OffPeakChargingPrice ||
                        c.Type == (int)ConfigurationTypeEnum.NormalChargingPrice ||
                        c.Type == (int)ConfigurationTypeEnum.PeakChargingPrice)
                        && !c.IsDeleted)
            .ToListAsync();

        if (!chargingConfigs.Any())
            throw new Exception("Chưa cấu hình bảng giá sạc điện trong hệ thống");

        
        var dayOfWeek = (int)chargingDate.DayOfWeek; // 0=Sunday, 1=Monday,...
        var hour = chargingDate.Hour;
        var minute = chargingDate.Minute;
        var totalMinutes = hour * 60 + minute;

        foreach (var config in chargingConfigs.OrderByDescending(c => c.Type))
        {
            try
            {
                var timeConfig = JsonSerializer.Deserialize<ChargingTimeConfiguration>(config.Description);

                if (timeConfig == null) continue;

                
                if (!timeConfig.DaysOfWeek.Contains(dayOfWeek))
                    continue;

                
                bool isInRange = timeConfig.TimeRanges.Any(tr => tr.IsInRange(totalMinutes));

                if (isInRange)
                {
                    
                    if (!decimal.TryParse(config.Value, out decimal rate))
                        throw new Exception($"Giá trị cấu hình không hợp lệ cho {config.Title}");

                    return (config.Title, rate, config.Description);
                }
            }
            catch (JsonException)
            {
                
                continue;
            }
        }

        
        throw new Exception($"Không tìm thấy cấu hình giá sạc cho thời điểm {chargingDate:yyyy-MM-dd HH:mm}");
    }

    private async Task<string> GetTimeSlotFromRateAsync(decimal rate)
    {
        var config = await _unitOfWork.GetConfigurationRepository()
            .Query()
            .Where(c => (c.Type == (int)ConfigurationTypeEnum.OffPeakChargingPrice ||
                        c.Type == (int)ConfigurationTypeEnum.NormalChargingPrice ||
                        c.Type == (int)ConfigurationTypeEnum.PeakChargingPrice)
                        && !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.Value == rate.ToString());

        return config?.Title ?? "N/A";
    }

    private string FormatChargingDescription(string jsonDescription, DateTime chargingDate)
    {
        try
        {
            var timeConfig = JsonSerializer.Deserialize<ChargingTimeConfiguration>(jsonDescription);
            if (timeConfig == null) return "Không có mô tả";

           
            var timeRangesText = string.Join(", ", timeConfig.TimeRanges.Select(tr => $"{tr.Start}-{tr.End}"));

            
            var daysText = FormatDaysOfWeek(timeConfig.DaysOfWeek);

            
            var currentDayText = GetVietnameseDayName(chargingDate.DayOfWeek);

            return $"Áp dụng: {timeRangesText} ({daysText}). Hôm nay là {currentDayText}.";
        }
        catch
        {
            return jsonDescription;
        }
    }


    private string FormatDaysOfWeek(List<int> daysOfWeek)
    {
        if (daysOfWeek.Count == 7)
            return "Tất cả các ngày";

        var dayNames = new Dictionary<int, string>
        {
            { 0, "CN" },
            { 1, "T2" },
            { 2, "T3" },
            { 3, "T4" },
            { 4, "T5" },
            { 5, "T6" },
            { 6, "T7" }
        };

        return string.Join(", ", daysOfWeek.Select(d => dayNames.GetValueOrDefault(d, "?")));
    }
    private string GetVietnameseDayName(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Sunday => "Chủ nhật",
            DayOfWeek.Monday => "Thứ hai",
            DayOfWeek.Tuesday => "Thứ ba",
            DayOfWeek.Wednesday => "Thứ tư",
            DayOfWeek.Thursday => "Thứ năm",
            DayOfWeek.Friday => "Thứ sáu",
            DayOfWeek.Saturday => "Thứ bảy",
            _ => "Không xác định"
        };
    }

    #endregion
}