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
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services
{
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
                // 1. Tìm xe theo biển số
                var vehicle = await _unitOfWork.GetVehicleRepository()
                    .Query()
                    .Include(v => v.VehicleModel)
                    .Include(v => v.Branch)
                    .Where(v => v.LicensePlate == licensePlate)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                    return ResultResponse<BookingChargingSearchResponse>.NotFound($"Không tìm thấy xe có biển số {licensePlate}");

                // 2. Tìm booking đang thuê xe này (Status = Renting)
                var booking = await _unitOfWork.GetBookingRepository()
                    .Query()
                    .Include(b => b.Renter)
                        .ThenInclude(r => r.Account)
                    .Include(b => b.VehicleModel)
                    .Include(b => b.RentalReceipts) // Để lấy Battery At Handover
                    .Where(b => b.VehicleId == vehicle.Id && b.BookingStatus == BookingStatusEnum.Renting.ToString())
                    .FirstOrDefaultAsync();

                if (booking == null)
                    return ResultResponse<BookingChargingSearchResponse>.NotFound($"Xe {licensePlate} hiện không có booking nào đang thuê");

                // 3. Lấy charging record gần nhất (nếu có)
                var lastChargingRecord = await _unitOfWork.GetChargingRecordRepository()
                    .GetLastChargingRecordByBookingIdAsync(booking.Id);

                // 4. Lấy battery at handover từ RentalReceipt (biên bản giao xe)
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
                var chargingRateResult = await DetermineChargingRateAsync(request.ChargingDate);
                var timeSlot = chargingRateResult.TimeSlot;
                var rate = chargingRateResult.Rate;
                var description = chargingRateResult.Description;

                var response = new ChargingRateResponse
                {
                    ChargingDate = request.ChargingDate,
                    TimeSlot = timeSlot,
                    RatePerKwh = rate,
                    Description = description
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

                // 1. Validate booking
                var booking = await _unitOfWork.GetBookingRepository().FindByIdAsync(request.BookingId);
                if (booking == null)
                    return ResultResponse<ChargingRecordResponse>.NotFound("Không tìm thấy booking");

                if (booking.BookingStatus != BookingStatusEnum.Renting.ToString())
                    return ResultResponse<ChargingRecordResponse>.Failure("Booking không ở trạng thái đang thuê");

                // 2. Validate battery percentage
                if (request.StartBatteryPercentage >= request.EndBatteryPercentage)
                    return ResultResponse<ChargingRecordResponse>.Failure("% pin sau sạc phải lớn hơn % pin trước sạc");

                if (request.KwhCharged <= 0)
                    return ResultResponse<ChargingRecordResponse>.Failure("Số kWh sạc phải lớn hơn 0");

                // 3. Get current staff info
                var staffId = Guid.Parse(_currentUserService.UserId);
                var staff = await _unitOfWork.GetStaffRepository().FindByIdAsync(staffId);
                if (staff == null)
                    return ResultResponse<ChargingRecordResponse>.Failure("Không tìm thấy thông tin nhân viên");

                // 4. Xác định khung giờ và giá điện
                var chargingRateResult = await DetermineChargingRateAsync(request.ChargingDate);
                var timeSlot = chargingRateResult.TimeSlot;
                var ratePerKwh = chargingRateResult.Rate;
                var description = chargingRateResult.Description;

                // 5. Tính phí
                var fee = request.KwhCharged * ratePerKwh;
                var batteryPercentageCharged = request.EndBatteryPercentage - request.StartBatteryPercentage;

                // 6. Tạo charging record
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

                // 7. Cập nhật TotalChargingFee của booking
                booking.TotalChargingFee += fee;
                _unitOfWork.GetBookingRepository().Update(booking);

                // 8. Save changes
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                // 9. Map response
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
                // 1. Get current renter ID
                var renterId = Guid.Parse(_currentUserService.UserId);

                // 2. Get all charging records của renter
                var chargingRecords = await _unitOfWork.GetChargingRecordRepository()
                    .GetChargingRecordsByRenterIdAsync(renterId);

                if (!chargingRecords.Any())
                    return ResultResponse<List<ChargingRecordListResponse>>.SuccessResult(
                        "Chưa có lịch sử sạc xe", new List<ChargingRecordListResponse>());

                // 3. Map to response
                var responseList = chargingRecords.Select(cr => new ChargingRecordListResponse
                {
                    Id = cr.Id,
                    ChargingDate = cr.ChargingDate ?? DateTime.UtcNow,
                    StartBatteryPercentage = cr.StartBatteryPercentage,
                    EndBatteryPercentage = cr.EndBatteryPercentage,
                    BatteryPercentageCharged = cr.EndBatteryPercentage - cr.StartBatteryPercentage,
                    KwhCharged = cr.KwhCharged,
                    RatePerKwh = cr.RatePerKwh,
                    Fee = cr.Fee,
                    TimeSlot = GetTimeSlotFromRate(cr.RatePerKwh),
                    Notes = cr.Notes,

                    // Booking info
                    BookingCode = cr.Booking.BookingCode,
                    VehicleModelName = cr.Booking.Vehicle?.VehicleModel?.ModelName ?? "N/A",
                    LicensePlate = cr.Booking.Vehicle?.LicensePlate ?? "N/A",

                    // Branch info
                    BranchName = cr.Branch.BranchName,
                    BranchAddress = cr.Branch.Address,

                    // Staff info
                    StaffName = cr.Staff.Account.Fullname ?? "N/A"
                }).ToList();

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
                // 1. Validate booking exists
                var booking = await _unitOfWork.GetBookingRepository()
                    .Query()
                    .Include(b => b.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Where(b => b.Id == bookingId)
                    .FirstOrDefaultAsync();

                if (booking == null)
                    return ResultResponse<List<ChargingRecordListResponse>>.NotFound("Không tìm thấy booking");

                // 2. Get all charging records by booking ID
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

                // 3. Map to response
                var responseList = chargingRecords.Select(cr => new ChargingRecordListResponse
                {
                    Id = cr.Id,
                    ChargingDate = cr.ChargingDate ?? DateTime.UtcNow,
                    StartBatteryPercentage = cr.StartBatteryPercentage,
                    EndBatteryPercentage = cr.EndBatteryPercentage,
                    BatteryPercentageCharged = cr.EndBatteryPercentage - cr.StartBatteryPercentage,
                    KwhCharged = cr.KwhCharged,
                    RatePerKwh = cr.RatePerKwh,
                    Fee = cr.Fee,
                    TimeSlot = GetTimeSlotFromRate(cr.RatePerKwh),
                    Notes = cr.Notes,

                    // Booking info
                    BookingCode = booking.BookingCode,
                    VehicleModelName = booking.Vehicle?.VehicleModel?.ModelName ?? "N/A",
                    LicensePlate = booking.Vehicle?.LicensePlate ?? "N/A",

                    // Branch info
                    BranchName = cr.Branch.BranchName,
                    BranchAddress = cr.Branch.Address,

                    // Staff info
                    StaffName = cr.Staff.Account.Fullname ?? "N/A"
                }).ToList();

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

        /// <summary>
        /// Xác định khung giờ và lấy giá điện từ Database (Configuration)
        /// </summary>
        private async Task<(string TimeSlot, decimal Rate, string Description)> DetermineChargingRateAsync(DateTime chargingDate)
        {
            // 1. Lấy tất cả config bảng giá sạc từ database
            var chargingConfigs = await _unitOfWork.GetConfigurationRepository()
                .Query()
                .Where(c => c.Type == (int)ConfigurationTypeEnum.ChargingRate && !c.IsDeleted)
                .ToListAsync();

            if (!chargingConfigs.Any())
                throw new Exception("Chưa cấu hình bảng giá sạc điện trong hệ thống");

            // 2. Xác định khung giờ dựa trên thời điểm
            var dayOfWeek = chargingDate.DayOfWeek;
            var hour = chargingDate.Hour;
            var minute = chargingDate.Minute;
            var totalMinutes = hour * 60 + minute;

            string timeSlotTitle;

            // Chủ nhật: Không có giờ cao điểm
            if (dayOfWeek == DayOfWeek.Sunday)
            {
                // 22:00 - 04:00: Giờ thấp điểm
                if (totalMinutes >= 1320 || totalMinutes < 240)
                    timeSlotTitle = "Giờ thấp điểm";
                else
                    timeSlotTitle = "Giờ bình thường";
            }
            else // Thứ 2 - Thứ 7
            {
                // Giờ thấp điểm: 22:00 - 04:00
                if (totalMinutes >= 1320 || totalMinutes < 240)
                    timeSlotTitle = "Giờ thấp điểm";
                // Giờ cao điểm: 09:30 - 11:30, 17:00 - 20:00
                else if ((totalMinutes >= 570 && totalMinutes < 690) || (totalMinutes >= 1020 && totalMinutes < 1200))
                    timeSlotTitle = "Giờ cao điểm";
                else
                    timeSlotTitle = "Giờ bình thường";
            }

            // 3. Tìm config tương ứng với khung giờ
            var config = chargingConfigs.FirstOrDefault(c => c.Title == timeSlotTitle);

            if (config == null)
                throw new Exception($"Không tìm thấy cấu hình giá cho {timeSlotTitle}");

            // 4. Parse giá từ Value
            if (!decimal.TryParse(config.Value, out decimal rate))
                throw new Exception($"Giá trị cấu hình không hợp lệ cho {timeSlotTitle}");

            return (timeSlotTitle, rate, config.Description);
        }

        /// <summary>
        /// Xác định tên khung giờ dựa trên giá (để hiển thị cho Renter)
        /// </summary>
        private string GetTimeSlotFromRate(decimal rate)
        {
            return rate switch
            {
                2850m => "Giờ thấp điểm",
                4650m => "Giờ bình thường",
                8100m => "Giờ cao điểm",
                _ => "N/A"
            };
        }

        #endregion
    }
}
