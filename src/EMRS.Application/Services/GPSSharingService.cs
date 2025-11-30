using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Abstractions.BackgroundJobs.GPSSharing;
using EMRS.Application.Common;
using EMRS.Application.DTOs.GPSSharingDTOs;
using EMRS.Application.DTOs.VehicleDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMRS.Application.Services
{
    public class GPSSharingService : IGPSSharingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFlespiService _flespiService;
        private readonly IGPSSharingJobScheduler _jobScheduler;

        public GPSSharingService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IFlespiService flespiService,
            IGPSSharingJobScheduler jobScheduler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _flespiService = flespiService;
            _jobScheduler = jobScheduler;
        }

        // ============================================
        // CREATE INVITATION
        // ============================================
        public async Task<ResultResponse<GPSSharingInviteResponse>> CreateInvitation(
            GPSSharingCreateRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var accountId = Guid.Parse(_currentUserService.UserId);

                // 1. Lấy Renter
                var renter = await _unitOfWork.GetRenterRepository()
                    .Query()
                    .Where(r => r.Id == accountId)
                    .FirstOrDefaultAsync();

                if (renter == null)
                    return ResultResponse<GPSSharingInviteResponse>.NotFound(
                        "Không tìm thấy thông tin khách thuê");

                // 2. Tìm Booking theo BookingId
                var activeBooking = await _unitOfWork.GetBookingRepository()
                    .Query()
                    .Include(b => b.Vehicle)
                    .Where(b => b.Id == request.BookingId
                        && b.RenterId == renter.Id
                        && b.BookingStatus == BookingStatusEnum.Renting.ToString())
                    .FirstOrDefaultAsync();

                if (activeBooking == null)
                    return ResultResponse<GPSSharingInviteResponse>.Failure(
                        "Booking không tồn tại hoặc không thuộc về bạn hoặc chưa bắt đầu thuê");

                // 3. Check: Vehicle đã được assign chưa
                if (activeBooking.VehicleId == null || activeBooking.Vehicle == null)
                    return ResultResponse<GPSSharingInviteResponse>.Failure(
                        "Booking chưa được giao xe, không thể tạo lời mời");

                // 4. Check: Booking này đã có invitation Pending chưa
                var existingInvitation = await _unitOfWork.GetGPSSharingRepository()
                    .GetPendingInvitationByBookingIdAsync(activeBooking.Id);

                if (existingInvitation != null)
                    return ResultResponse<GPSSharingInviteResponse>.Failure(
                        "Booking này đã có lời mời đang chờ. Vui lòng hủy hoặc đợi hết hạn.");

                // 5. Check: Renter đã có session Active chưa
                var activeSession = await _unitOfWork.GetGPSSharingRepository()
                    .GetActiveSessionByRenterIdAsync(renter.Id);

                if (activeSession != null)
                    return ResultResponse<GPSSharingInviteResponse>.Failure(
                        "Bạn đang có session sharing đang hoạt động");

                // 6. Generate unique invitation code
                var invitationCode = await GenerateUniqueInvitationCode();

                // 7. Tạo GPSSharing với BookingId
                var sharing = new GPSSharing
                {
                    InvitationCode = invitationCode,
                    Status = GPSSharingStatusEnum.Pending.ToString(),
                    ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30),
                    OwnerBookingId = activeBooking.Id
                };

                await _unitOfWork.GetGPSSharingRepository().AddAsync(sharing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                // ✅ 8. SCHEDULE AUTO-EXPIRE BẰNG HANGFIRE
                _jobScheduler.ScheduleAutoExpire(sharing.Id, TimeSpan.FromMinutes(30));

                var response = new GPSSharingInviteResponse
                {
                    SessionId = sharing.Id,
                    InvitationCode = invitationCode,
                    InvitationExpiresAt = sharing.ExpiresAt,
                    OwnerVehicleLicensePlate = activeBooking.Vehicle?.LicensePlate ?? "Unknown"
                };

                return ResultResponse<GPSSharingInviteResponse>.SuccessResult(
                    "Lời mời đã tạo thành công. Hết hạn sau 30 phút.", response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<GPSSharingInviteResponse>.Failure(
                    $"Lỗi khi tạo lời mời: {ex.Message}");
            }
        }

        // ============================================
        // JOIN SHARING
        // ============================================
        public async Task<ResultResponse<GPSSharingActiveResponse>> JoinSharing(
            GPSSharingJoinRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var accountId = Guid.Parse(_currentUserService.UserId);

                // 1. Lấy Renter
                var guestRenter = await _unitOfWork.GetRenterRepository()
                    .Query()
                    .Where(r => r.Id == accountId)
                    .FirstOrDefaultAsync();

                if (guestRenter == null)
                    return ResultResponse<GPSSharingActiveResponse>.NotFound(
                        "Không tìm thấy thông tin khách thuê");

                // 2. Tìm invitation
                var sharing = await _unitOfWork.GetGPSSharingRepository()
                    .Query()
                    .Include(s => s.OwnerBooking)
                        .ThenInclude(b => b.Renter)
                    .Include(s => s.OwnerBooking)
                        .ThenInclude(b => b.Vehicle)
                    .Where(s => s.InvitationCode == request.InvitationCode)
                    .FirstOrDefaultAsync();

                if (sharing == null)
                    return ResultResponse<GPSSharingActiveResponse>.NotFound(
                        "Mã lời mời không tồn tại");

                // 3. Validate status
                if (sharing.Status != GPSSharingStatusEnum.Pending.ToString())
                    return ResultResponse<GPSSharingActiveResponse>.Failure(
                        "Lời mời đã được sử dụng hoặc hết hạn");

                // 4. Check invitation expiry
                if (DateTimeOffset.UtcNow > sharing.ExpiresAt)
                {
                    sharing.Status = GPSSharingStatusEnum.Expired.ToString();
                    _unitOfWork.GetGPSSharingRepository().Update(sharing);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                    return ResultResponse<GPSSharingActiveResponse>.Failure(
                        "Lời mời đã hết hạn");
                }

                // 5. Validate: Guest không thể là Owner
                if (sharing.OwnerBooking.RenterId == guestRenter.Id)
                    return ResultResponse<GPSSharingActiveResponse>.Failure(
                        "Bạn không thể tham gia session của chính mình");

                // 6. Tìm Booking của Guest theo GuestBookingId
                var guestBooking = await _unitOfWork.GetBookingRepository()
                    .Query()
                    .Include(b => b.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(b => b.Renter)
                        .ThenInclude(r => r.Account)
                    .Where(b => b.Id == request.GuestBookingId
                        && b.RenterId == guestRenter.Id
                        && b.BookingStatus == BookingStatusEnum.Renting.ToString())
                    .FirstOrDefaultAsync();

                if (guestBooking == null)
                    return ResultResponse<GPSSharingActiveResponse>.Failure(
                        "Booking không tồn tại hoặc không thuộc về bạn hoặc chưa bắt đầu thuê");

                // 7. Check: Vehicle đã được assign chưa
                if (guestBooking.VehicleId == null || guestBooking.Vehicle == null)
                    return ResultResponse<GPSSharingActiveResponse>.Failure(
                        "Booking chưa được giao xe, không thể tham gia sharing");

                // 8. Check: Guest đã có session Active chưa
                var existingGuestSession = await _unitOfWork.GetGPSSharingRepository()
                    .GetActiveSessionByRenterIdAsync(guestRenter.Id);

                if (existingGuestSession != null)
                    return ResultResponse<GPSSharingActiveResponse>.Failure(
                        "Bạn đang có session sharing khác đang hoạt động");

                // 9. Update sharing với GuestBookingId
                sharing.GuestBookingId = guestBooking.Id;
                sharing.Status = GPSSharingStatusEnum.Active.ToString();
                sharing.AcceptedAt = DateTimeOffset.UtcNow;
                sharing.SessionExpiresAt = DateTimeOffset.UtcNow.AddHours(24);

                _unitOfWork.GetGPSSharingRepository().Update(sharing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                // 10. Tạo tokens
                var ownerVehicle = sharing.OwnerBooking.Vehicle!;
                var guestVehicle = guestBooking.Vehicle!;

                var ownerToken = await _flespiService.CreateFlespiAclTokenAsync(
                    ownerVehicle, ttlSeconds: 86400, minutes: 1440);
                var guestToken = await _flespiService.CreateFlespiAclTokenAsync(
                    guestVehicle, ttlSeconds: 86400, minutes: 1440);

                // 11. Build response
                var response = new GPSSharingActiveResponse
                {
                    SessionId = sharing.Id,
                    SessionExpiresAt = sharing.SessionExpiresAt.Value,

                    OwnerInfo = new ParticipantTrackingInfo
                    {
                        RenterId = sharing.OwnerBooking.RenterId,
                        RenterName = sharing.OwnerBooking.Renter.Account?.Username ?? "Unknown",
                        Vehicle = new VehicleTrackingResponse
                        {
                            Id = ownerVehicle.Id,
                            LicensePlate = ownerVehicle.LicensePlate,
                            Color = ownerVehicle.Color,
                            YearOfManufacture = ownerVehicle.YearOfManufacture,
                            CurrentOdometerKm = ownerVehicle.CurrentOdometerKm,
                            BatteryHealthPercentage = ownerVehicle.BatteryHealthPercentage,
                            Status = ownerVehicle.Status,
                            PurchaseDate = ownerVehicle.PurchaseDate,
                            Description = ownerVehicle.Description,
                            tempTrackingPayload = ownerToken
                        }
                    },

                    GuestInfo = new ParticipantTrackingInfo
                    {
                        RenterId = guestBooking.RenterId,
                        RenterName = guestBooking.Renter.Account?.Username ?? _currentUserService.Username,
                        Vehicle = new VehicleTrackingResponse
                        {
                            Id = guestVehicle.Id,
                            LicensePlate = guestVehicle.LicensePlate,
                            Color = guestVehicle.Color,
                            YearOfManufacture = guestVehicle.YearOfManufacture,
                            CurrentOdometerKm = guestVehicle.CurrentOdometerKm,
                            BatteryHealthPercentage = guestVehicle.BatteryHealthPercentage,
                            Status = guestVehicle.Status,
                            PurchaseDate = guestVehicle.PurchaseDate,
                            Description = guestVehicle.Description,
                            tempTrackingPayload = guestToken
                        }
                    }
                };

                return ResultResponse<GPSSharingActiveResponse>.SuccessResult(
                    "Đã kết nối thành công. Session hết hạn sau 24 giờ.", response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<GPSSharingActiveResponse>.Failure(
                    $"Lỗi khi tham gia session: {ex.Message}");
            }
        }

        // ============================================
        // ✅ ĐỔI TÊN: GET SESSIONS (TẤT CẢ TRẠNG THÁI)
        // Trước: GetActiveSession() - Chỉ lấy Active
        // Sau: GetSessions() - Lấy tất cả
        // ============================================
        public async Task<ResultResponse<List<GPSSharingSessionResponse>>> GetSessions()
        {
            try
            {
                var accountId = Guid.Parse(_currentUserService.UserId);

                var renter = await _unitOfWork.GetRenterRepository()
                    .Query()
                    .Where(r => r.Id == accountId)
                    .FirstOrDefaultAsync();

                if (renter == null)
                    return ResultResponse<List<GPSSharingSessionResponse>>.NotFound(
                        "Không tìm thấy thông tin khách thuê");

                // Lấy TẤT CẢ sessions (không filter theo status)
                var sessions = await _unitOfWork.GetGPSSharingRepository()
                    .Query()
                    .Include(s => s.OwnerBooking)
                        .ThenInclude(b => b.Renter)
                            .ThenInclude(r => r.Account)
                    .Include(s => s.OwnerBooking)
                        .ThenInclude(b => b.Vehicle)
                    .Include(s => s.GuestBooking)
                        .ThenInclude(b => b.Renter)
                            .ThenInclude(r => r.Account)
                    .Include(s => s.GuestBooking)
                        .ThenInclude(b => b.Vehicle)
                    .Where(s => (s.OwnerBooking.RenterId == renter.Id
                            || (s.GuestBookingId != null && s.GuestBooking!.RenterId == renter.Id))
                        && !s.IsDeleted)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();

                var responseList = new List<GPSSharingSessionResponse>();

                foreach (var sharing in sessions)
                {
                    // Nếu session đã kết thúc → Không trả token
                    if (sharing.Status != GPSSharingStatusEnum.Active.ToString())
                    {
                        responseList.Add(new GPSSharingSessionResponse
                        {
                            SessionId = sharing.Id,
                            InvitationCode = sharing.InvitationCode,
                            Status = sharing.Status,
                            InvitationExpiresAt = sharing.ExpiresAt,
                            SessionExpiresAt = sharing.SessionExpiresAt,
                            AcceptedAt = sharing.AcceptedAt,

                            OwnerInfo = new ParticipantTrackingInfo
                            {
                                RenterId = sharing.OwnerBooking.RenterId,
                                RenterName = sharing.OwnerBooking.Renter.Account?.Username ?? "Unknown",
                                Vehicle = new VehicleTrackingResponse
                                {
                                    Id = sharing.OwnerBooking.Vehicle!.Id,
                                    LicensePlate = sharing.OwnerBooking.Vehicle.LicensePlate,
                                    Color = sharing.OwnerBooking.Vehicle.Color,
                                    Status = sharing.OwnerBooking.Vehicle.Status
                                }
                            },

                            GuestInfo = sharing.GuestBooking != null ? new ParticipantTrackingInfo
                            {
                                RenterId = sharing.GuestBooking.RenterId,
                                RenterName = sharing.GuestBooking.Renter.Account?.Username ?? "Unknown",
                                Vehicle = new VehicleTrackingResponse
                                {
                                    Id = sharing.GuestBooking.Vehicle!.Id,
                                    LicensePlate = sharing.GuestBooking.Vehicle.LicensePlate,
                                    Color = sharing.GuestBooking.Vehicle.Color,
                                    Status = sharing.GuestBooking.Vehicle.Status
                                }
                            } : null
                        });
                    }
                    else
                    {
                        // Session Active → Tạo tokens
                        var ownerVehicle = sharing.OwnerBooking.Vehicle!;
                        var guestVehicle = sharing.GuestBooking!.Vehicle!;

                        var ownerToken = await _flespiService.CreateFlespiAclTokenAsync(
                            ownerVehicle, ttlSeconds: 86400, minutes: 1440);
                        var guestToken = await _flespiService.CreateFlespiAclTokenAsync(
                            guestVehicle, ttlSeconds: 86400, minutes: 1440);

                        responseList.Add(new GPSSharingSessionResponse
                        {
                            SessionId = sharing.Id,
                            InvitationCode = sharing.InvitationCode,
                            Status = sharing.Status,
                            SessionExpiresAt = sharing.SessionExpiresAt,
                            AcceptedAt = sharing.AcceptedAt,

                            OwnerInfo = new ParticipantTrackingInfo
                            {
                                RenterId = sharing.OwnerBooking.RenterId,
                                RenterName = sharing.OwnerBooking.Renter.Account?.Username ?? "Unknown",
                                Vehicle = new VehicleTrackingResponse
                                {
                                    Id = ownerVehicle.Id,
                                    LicensePlate = ownerVehicle.LicensePlate,
                                    Color = ownerVehicle.Color,
                                    YearOfManufacture = ownerVehicle.YearOfManufacture,
                                    CurrentOdometerKm = ownerVehicle.CurrentOdometerKm,
                                    BatteryHealthPercentage = ownerVehicle.BatteryHealthPercentage,
                                    Status = ownerVehicle.Status,
                                    PurchaseDate = ownerVehicle.PurchaseDate,
                                    Description = ownerVehicle.Description,
                                    tempTrackingPayload = ownerToken
                                }
                            },

                            GuestInfo = new ParticipantTrackingInfo
                            {
                                RenterId = sharing.GuestBooking.RenterId,
                                RenterName = sharing.GuestBooking.Renter.Account?.Username ?? "Unknown",
                                Vehicle = new VehicleTrackingResponse
                                {
                                    Id = guestVehicle.Id,
                                    LicensePlate = guestVehicle.LicensePlate,
                                    Color = guestVehicle.Color,
                                    YearOfManufacture = guestVehicle.YearOfManufacture,
                                    CurrentOdometerKm = guestVehicle.CurrentOdometerKm,
                                    BatteryHealthPercentage = guestVehicle.BatteryHealthPercentage,
                                    Status = guestVehicle.Status,
                                    PurchaseDate = guestVehicle.PurchaseDate,
                                    Description = guestVehicle.Description,
                                    tempTrackingPayload = guestToken
                                }
                            }
                        });
                    }
                }

                return ResultResponse<List<GPSSharingSessionResponse>>.SuccessResult(
                    $"Tìm thấy {responseList.Count} session(s)", responseList);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<GPSSharingSessionResponse>>.Failure(
                    $"Lỗi: {ex.Message}");
            }
        }

        // ============================================
        // CANCEL SESSION
        // ============================================
        public async Task<ResultResponse<bool>> CancelSession(Guid sessionId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var accountId = Guid.Parse(_currentUserService.UserId);

                var renter = await _unitOfWork.GetRenterRepository()
                    .Query()
                    .Where(r => r.Id == accountId)
                    .FirstOrDefaultAsync();

                if (renter == null)
                    return ResultResponse<bool>.NotFound(
                        "Không tìm thấy thông tin khách thuê");

                var sharing = await _unitOfWork.GetGPSSharingRepository()
                    .Query()
                    .Include(s => s.OwnerBooking)
                    .Include(s => s.GuestBooking)
                    .Where(s => s.Id == sessionId && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (sharing == null)
                    return ResultResponse<bool>.NotFound("Session không tồn tại");

                // Validate quyền: Chỉ Owner hoặc Guest mới cancel được
                var isOwner = sharing.OwnerBooking.RenterId == renter.Id;
                var isGuest = sharing.GuestBookingId != null
                    && sharing.GuestBooking!.RenterId == renter.Id;

                if (!isOwner && !isGuest)
                    return ResultResponse<bool>.Forbidden(
                        "Bạn không có quyền hủy session này");

                // Chỉ cancel được nếu đang Pending hoặc Active
                if (sharing.Status != GPSSharingStatusEnum.Pending.ToString()
                    && sharing.Status != GPSSharingStatusEnum.Active.ToString())
                {
                    return ResultResponse<bool>.Failure(
                        "Session đã kết thúc, không thể hủy");
                }

                sharing.Status = GPSSharingStatusEnum.Cancelled.ToString();
                _unitOfWork.GetGPSSharingRepository().Update(sharing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return ResultResponse<bool>.SuccessResult(
                    "Đã hủy session thành công", true);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<bool>.Failure($"Lỗi: {ex.Message}");
            }
        }

        // ============================================
        // GET SESSION DETAIL
        // ============================================
        public async Task<ResultResponse<GPSSharingSessionResponse>> GetSessionDetail(
            Guid sessionId)
        {
            try
            {
                var accountId = Guid.Parse(_currentUserService.UserId);

                var renter = await _unitOfWork.GetRenterRepository()
                    .Query()
                    .Where(r => r.Id == accountId)
                    .FirstOrDefaultAsync();

                if (renter == null)
                    return ResultResponse<GPSSharingSessionResponse>.NotFound(
                        "Không tìm thấy thông tin khách thuê");

                var sharing = await _unitOfWork.GetGPSSharingRepository()
                    .GetSessionWithDetailsAsync(sessionId);

                if (sharing == null)
                    return ResultResponse<GPSSharingSessionResponse>.NotFound(
                        "Session không tồn tại");

                // Validate quyền truy cập qua Booking
                var isOwner = sharing.OwnerBooking.RenterId == renter.Id;
                var isGuest = sharing.GuestBookingId != null
                    && sharing.GuestBooking!.RenterId == renter.Id;

                if (!isOwner && !isGuest)
                    return ResultResponse<GPSSharingSessionResponse>.Forbidden(
                        "Bạn không có quyền truy cập session này");

                // Nếu session đã kết thúc → Không trả token
                if (sharing.Status != GPSSharingStatusEnum.Active.ToString())
                {
                    return ResultResponse<GPSSharingSessionResponse>.SuccessResult(
                        "Session đã kết thúc",
                        new GPSSharingSessionResponse
                        {
                            SessionId = sharing.Id,
                            InvitationCode = sharing.InvitationCode,
                            Status = sharing.Status,
                            InvitationExpiresAt = sharing.ExpiresAt,
                            SessionExpiresAt = sharing.SessionExpiresAt,
                            AcceptedAt = sharing.AcceptedAt,

                            OwnerInfo = new ParticipantTrackingInfo
                            {
                                RenterId = sharing.OwnerBooking.RenterId,
                                RenterName = sharing.OwnerBooking.Renter.Account?.Username ?? "Unknown",
                                Vehicle = new VehicleTrackingResponse
                                {
                                    Id = sharing.OwnerBooking.Vehicle!.Id,
                                    LicensePlate = sharing.OwnerBooking.Vehicle.LicensePlate,
                                    Color = sharing.OwnerBooking.Vehicle.Color,
                                    Status = sharing.OwnerBooking.Vehicle.Status
                                }
                            },

                            GuestInfo = sharing.GuestBooking != null ? new ParticipantTrackingInfo
                            {
                                RenterId = sharing.GuestBooking.RenterId,
                                RenterName = sharing.GuestBooking.Renter.Account?.Username ?? "Unknown",
                                Vehicle = new VehicleTrackingResponse
                                {
                                    Id = sharing.GuestBooking.Vehicle!.Id,
                                    LicensePlate = sharing.GuestBooking.Vehicle.LicensePlate,
                                    Color = sharing.GuestBooking.Vehicle.Color,
                                    Status = sharing.GuestBooking.Vehicle.Status
                                }
                            } : null
                        });
                }

                // Session Active → Tạo tokens
                var ownerVehicle = sharing.OwnerBooking.Vehicle!;
                var guestVehicle = sharing.GuestBooking!.Vehicle!;

                var ownerToken = await _flespiService.CreateFlespiAclTokenAsync(
                    ownerVehicle, ttlSeconds: 86400, minutes: 1440);
                var guestToken = await _flespiService.CreateFlespiAclTokenAsync(
                    guestVehicle, ttlSeconds: 86400, minutes: 1440);

                var response = new GPSSharingSessionResponse
                {
                    SessionId = sharing.Id,
                    InvitationCode = sharing.InvitationCode,
                    Status = sharing.Status,
                    SessionExpiresAt = sharing.SessionExpiresAt,
                    AcceptedAt = sharing.AcceptedAt,

                    OwnerInfo = new ParticipantTrackingInfo
                    {
                        RenterId = sharing.OwnerBooking.RenterId,
                        RenterName = sharing.OwnerBooking.Renter.Account?.Username ?? "Unknown",
                        Vehicle = new VehicleTrackingResponse
                        {
                            Id = ownerVehicle.Id,
                            LicensePlate = ownerVehicle.LicensePlate,
                            Color = ownerVehicle.Color,
                            YearOfManufacture = ownerVehicle.YearOfManufacture,
                            CurrentOdometerKm = ownerVehicle.CurrentOdometerKm,
                            BatteryHealthPercentage = ownerVehicle.BatteryHealthPercentage,
                            Status = ownerVehicle.Status,
                            PurchaseDate = ownerVehicle.PurchaseDate,
                            Description = ownerVehicle.Description,
                            tempTrackingPayload = ownerToken
                        }
                    },

                    GuestInfo = new ParticipantTrackingInfo
                    {
                        RenterId = sharing.GuestBooking.RenterId,
                        RenterName = sharing.GuestBooking.Renter.Account?.Username ?? "Unknown",
                        Vehicle = new VehicleTrackingResponse
                        {
                            Id = guestVehicle.Id,
                            LicensePlate = guestVehicle.LicensePlate,
                            Color = guestVehicle.Color,
                            YearOfManufacture = guestVehicle.YearOfManufacture,
                            CurrentOdometerKm = guestVehicle.CurrentOdometerKm,
                            BatteryHealthPercentage = guestVehicle.BatteryHealthPercentage,
                            Status = guestVehicle.Status,
                            PurchaseDate = guestVehicle.PurchaseDate,
                            Description = guestVehicle.Description,
                            tempTrackingPayload = guestToken
                        }
                    }
                };

                return ResultResponse<GPSSharingSessionResponse>.SuccessResult(
                    "Lấy session thành công", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<GPSSharingSessionResponse>.Failure(
                    $"Lỗi: {ex.Message}");
            }
        }

        // ============================================
        // ✅ ĐỔI TÊN: GET SESSIONS BY RENTER ID
        // Trước: GetMySessionHistory() - Current user only
        // Sau: GetSessionsByRenterId(Guid renterId) - Query bất kỳ renter
        // ============================================
        public async Task<ResultResponse<List<GPSSharingHistoryResponse>>> GetSessionsByRenterId(
            Guid renterId)
        {
            try
            {
                // Check: Renter có tồn tại không
                var renter = await _unitOfWork.GetRenterRepository()
                    .FindByIdAsync(renterId);

                if (renter == null)
                    return ResultResponse<List<GPSSharingHistoryResponse>>.NotFound(
                        "Renter không tồn tại");

                // Lấy tất cả sessions của renter này
                var sessions = await _unitOfWork.GetGPSSharingRepository()
                    .Query()
                    .Include(s => s.OwnerBooking)
                        .ThenInclude(b => b.Renter)
                            .ThenInclude(r => r.Account)
                    .Include(s => s.OwnerBooking)
                        .ThenInclude(b => b.Vehicle)
                    .Include(s => s.GuestBooking)
                        .ThenInclude(b => b.Renter)
                            .ThenInclude(r => r.Account)
                    .Include(s => s.GuestBooking)
                        .ThenInclude(b => b.Vehicle)
                    .Where(s => (s.OwnerBooking.RenterId == renterId
                            || (s.GuestBookingId != null && s.GuestBooking!.RenterId == renterId))
                        && !s.IsDeleted)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();

                var response = sessions.Select(session => new GPSSharingHistoryResponse
                {
                    SessionId = session.Id,
                    InvitationCode = session.InvitationCode,
                    Status = session.Status,
                    CreatedAt = session.CreatedAt,
                    AcceptedAt = session.AcceptedAt,
                    SessionExpiresAt = session.SessionExpiresAt,

                    OwnerRenterId = session.OwnerBooking.RenterId,
                    OwnerRenterName = session.OwnerBooking.Renter.Account?.Username ?? "Unknown",
                    OwnerVehicleLicensePlate = session.OwnerBooking.Vehicle?.LicensePlate ?? "Unknown",

                    GuestRenterId = session.GuestBooking?.RenterId,
                    GuestRenterName = session.GuestBooking?.Renter.Account?.Username,
                    GuestVehicleLicensePlate = session.GuestBooking?.Vehicle?.LicensePlate
                }).ToList();

                return ResultResponse<List<GPSSharingHistoryResponse>>.SuccessResult(
                    $"Tìm thấy {response.Count} session(s)", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<GPSSharingHistoryResponse>>.Failure(
                    $"Lỗi: {ex.Message}");
            }
        }

        // ============================================
        // GET ALL SESSIONS (FOR MANAGER/ADMIN)
        // ============================================
        public async Task<ResultResponse<List<GPSSharingHistoryResponse>>> GetAllSessions()
        {
            try
            {
                var sessions = await _unitOfWork.GetGPSSharingRepository()
                    .GetAllSessionsForHistoryAsync();

                var response = sessions.Select(session => new GPSSharingHistoryResponse
                {
                    SessionId = session.Id,
                    InvitationCode = session.InvitationCode,
                    Status = session.Status,
                    CreatedAt = session.CreatedAt,
                    AcceptedAt = session.AcceptedAt,
                    SessionExpiresAt = session.SessionExpiresAt,

                    OwnerRenterId = session.OwnerBooking.RenterId,
                    OwnerRenterName = session.OwnerBooking.Renter.Account?.Username ?? "Unknown",
                    OwnerVehicleLicensePlate = session.OwnerBooking.Vehicle?.LicensePlate ?? "Unknown",

                    GuestRenterId = session.GuestBooking?.RenterId,
                    GuestRenterName = session.GuestBooking?.Renter.Account?.Username,
                    GuestVehicleLicensePlate = session.GuestBooking?.Vehicle?.LicensePlate
                }).ToList();

                return ResultResponse<List<GPSSharingHistoryResponse>>.SuccessResult(
                    "Lấy danh sách tất cả sessions thành công", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<GPSSharingHistoryResponse>>.Failure(
                    $"Lỗi: {ex.Message}");
            }
        }

        // ============================================
        // HELPER: GENERATE UNIQUE INVITATION CODE
        // ============================================
        private async Task<string> GenerateUniqueInvitationCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            string code;

            do
            {
                code = new string(Enumerable.Range(0, 6)
                    .Select(_ => chars[random.Next(chars.Length)])
                    .ToArray());

                var existing = await _unitOfWork.GetGPSSharingRepository()
                    .GetByInvitationCodeAsync(code);

                if (existing == null) break;

            } while (true);

            return code;
        }
    }
}