using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Abstractions.BackgroundJobs.GPSSharing;
using EMRS.Application.Abstractions.Models.Protrack;
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
        private readonly ICurrentUserService _currentUserService;
        private readonly IFlespiService _flespiService;
        private readonly IGPSSharingJobScheduler _jobScheduler;

        public GPSSharingService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IFlespiService flespiService,
            IGPSSharingJobScheduler jobScheduler)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _flespiService = flespiService;
            _jobScheduler = jobScheduler;
        }

        public async Task<ResultResponse<GPSSharingInviteResponse>> CreateInvitation(
            GPSSharingCreateRequest request)
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
                    return ResultResponse<GPSSharingInviteResponse>.NotFound(
                        "Không tìm thấy thông tin khách thuê");

                
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

                
                if (activeBooking.VehicleId == null || activeBooking.Vehicle == null)
                    return ResultResponse<GPSSharingInviteResponse>.Failure(
                        "Booking chưa được giao xe, không thể tạo lời mời");

                
                var existingInvitation = await _unitOfWork.GetGPSSharingRepository()
                    .GetPendingInvitationByBookingIdAsync(activeBooking.Id);

                if (existingInvitation != null)
                    return ResultResponse<GPSSharingInviteResponse>.Failure(
                        "Booking này đã có lời mời đang chờ. Vui lòng hủy hoặc đợi hết hạn.");

                
                var activeSession = await _unitOfWork.GetGPSSharingRepository()
                    .GetActiveSessionByRenterIdAsync(renter.Id);

                if (activeSession != null)
                    return ResultResponse<GPSSharingInviteResponse>.Failure(
                        "Bạn đang có session sharing đang hoạt động");

                
                var invitationCode = await GenerateUniqueInvitationCode();

               
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

        public async Task<ResultResponse<GPSSharingActiveResponse>> JoinSharing(
            GPSSharingJoinRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var accountId = Guid.Parse(_currentUserService.UserId);

                
                var guestRenter = await _unitOfWork.GetRenterRepository()
                    .Query()
                    .Where(r => r.Id == accountId)
                    .FirstOrDefaultAsync();

                if (guestRenter == null)
                    return ResultResponse<GPSSharingActiveResponse>.NotFound(
                        "Không tìm thấy thông tin khách thuê");

                
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

                
                if (sharing.Status != GPSSharingStatusEnum.Pending.ToString())
                    return ResultResponse<GPSSharingActiveResponse>.Failure(
                        "Lời mời đã được sử dụng hoặc hết hạn");

                
                if (DateTimeOffset.UtcNow > sharing.ExpiresAt)
                {
                    sharing.Status = GPSSharingStatusEnum.Expired.ToString();
                    _unitOfWork.GetGPSSharingRepository().Update(sharing);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                    return ResultResponse<GPSSharingActiveResponse>.Failure(
                        "Lời mời đã hết hạn");
                }

                
                if (sharing.OwnerBooking.RenterId == guestRenter.Id)
                    return ResultResponse<GPSSharingActiveResponse>.Failure(
                        "Bạn không thể tham gia session của chính mình");

                
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

                
                if (guestBooking.VehicleId == null || guestBooking.Vehicle == null)
                    return ResultResponse<GPSSharingActiveResponse>.Failure(
                        "Booking chưa được giao xe, không thể tham gia sharing");

                
                var existingGuestSession = await _unitOfWork.GetGPSSharingRepository()
                    .GetActiveSessionByRenterIdAsync(guestRenter.Id);

                if (existingGuestSession != null)
                    return ResultResponse<GPSSharingActiveResponse>.Failure(
                        "Bạn đang có session sharing khác đang hoạt động");

                var ownerVehicle = sharing.OwnerBooking.Vehicle!;
                var guestVehicle = guestBooking.Vehicle!;

                var ownerToken = await _flespiService.CreateFlespiAclTokenAsync(
                    ownerVehicle, ttlSeconds: 7200, minutes: 120);
                var guestToken = await _flespiService.CreateFlespiAclTokenAsync(
                    guestVehicle, ttlSeconds: 7200, minutes: 120);

                var ownerAvatar = await _unitOfWork.GetMediaRepository()
                    .Query()
                    .Where(m => m.DocNo == sharing.OwnerBooking.RenterId
                        && m.EntityType == MediaEntityTypeEnum.Renter.ToString())
                    .Select(m => m.FileUrl)
                    .FirstOrDefaultAsync();

                var guestAvatar = await _unitOfWork.GetMediaRepository()
                    .Query()
                    .Where(m => m.DocNo == guestBooking.RenterId
                        && m.EntityType == MediaEntityTypeEnum.Renter.ToString())
                    .Select(m => m.FileUrl)
                    .FirstOrDefaultAsync();

                sharing.GuestBookingId = guestBooking.Id;
                sharing.Status = GPSSharingStatusEnum.Active.ToString();
                sharing.AcceptedAt = DateTimeOffset.UtcNow;
                sharing.SessionExpiresAt = DateTimeOffset.UtcNow.AddHours(2);
                sharing.OwnerTokenSharing = ownerToken.tmpToken;
                sharing.GuestTokenSharing = guestToken.tmpToken;

                _unitOfWork.GetGPSSharingRepository().Update(sharing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                
                var response = new GPSSharingActiveResponse
                {
                    SessionId = sharing.Id,
                    SessionExpiresAt = sharing.SessionExpiresAt.Value,

                    OwnerInfo = new ParticipantTrackingInfo
                    {
                        RenterId = sharing.OwnerBooking.RenterId,
                        RenterName = sharing.OwnerBooking.Renter.Account?.Username ?? "Unknown",
                        AvatarRenter = ownerAvatar,
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
                        AvatarRenter = guestAvatar,
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
                    "Đã kết nối thành công. Session hết hạn sau 2 giờ.", response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<GPSSharingActiveResponse>.Failure(
                    $"Lỗi khi tham gia session: {ex.Message}");
            }
        }


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

                
                var isOwner = sharing.OwnerBooking.RenterId == renter.Id;
                var isGuest = sharing.GuestBookingId != null
                    && sharing.GuestBooking!.RenterId == renter.Id;

                if (!isOwner && !isGuest)
                    return ResultResponse<bool>.Forbidden(
                        "Bạn không có quyền hủy session này");

                
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

                // ✅ FIX 1: Dùng query trực tiếp với AsNoTracking thay vì GetSessionWithDetailsAsync
                var sharing = await _unitOfWork.GetGPSSharingRepository()
                    .Query()
                    .AsNoTracking() 
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
                    .Where(s => s.Id == sessionId && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (sharing == null)
                    return ResultResponse<GPSSharingSessionResponse>.NotFound(
                        "Session không tồn tại");


                var isOwner = sharing.OwnerBooking.RenterId == renter.Id;
                var isGuest = sharing.GuestBookingId != null
                    && sharing.GuestBooking!.RenterId == renter.Id;

                if (!isOwner && !isGuest)
                    return ResultResponse<GPSSharingSessionResponse>.Forbidden(
                        "Bạn không có quyền truy cập session này");

                var ownerAvatar = await _unitOfWork.GetMediaRepository()
                    .Query()
                    .Where(m => m.DocNo == sharing.OwnerBooking.RenterId
                        && m.EntityType == MediaEntityTypeEnum.Renter.ToString())
                    .Select(m => m.FileUrl)
                    .FirstOrDefaultAsync();

                string? guestAvatar = null;
                if (sharing.GuestBookingId != null)
                {
                    guestAvatar = await _unitOfWork.GetMediaRepository()
                        .Query()
                        .Where(m => m.DocNo == sharing.GuestBooking!.RenterId
                            && m.EntityType == MediaEntityTypeEnum.Renter.ToString())
                        .Select(m => m.FileUrl)
                        .FirstOrDefaultAsync();
                }

                // Nếu session không Active → Trả về không có token
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
                            AvatarOwner = ownerAvatar,
                            AvatarGuest = guestAvatar,
                            OwnerInfo = new ParticipantTrackingInfo
                            {
                                RenterId = sharing.OwnerBooking.RenterId,
                                RenterName = sharing.OwnerBooking.Renter.Account?.Username ?? "Unknown",
                                AvatarRenter = ownerAvatar,
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
                                AvatarRenter = guestAvatar,
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

                //  Session Active → Check tokens có hết hạn không
                var ownerVehicle = sharing.OwnerBooking.Vehicle!;
                var guestVehicle = sharing.GuestBooking!.Vehicle!;

                string ownerTokenValue;
                string guestTokenValue;

                bool tokensExpired = sharing.SessionExpiresAt == null
                    || DateTimeOffset.UtcNow >= sharing.SessionExpiresAt.Value;

                if (tokensExpired || string.IsNullOrEmpty(sharing.OwnerTokenSharing)
                    || string.IsNullOrEmpty(sharing.GuestTokenSharing))
                {
                    //  Tokens hết hạn hoặc chưa có thì TẠO MỚI
                    var ownerToken = await _flespiService.CreateFlespiAclTokenAsync(
                        ownerVehicle, ttlSeconds: 7200, minutes: 120);
                    var guestToken = await _flespiService.CreateFlespiAclTokenAsync(
                        guestVehicle, ttlSeconds: 7200, minutes: 120);

                    // ✅ FIX 2: Query đơn giản CHỈ lấy GPSSharing, không include navigation
                    var trackableSharing = await _unitOfWork.GetGPSSharingRepository()
                        .Query()
                        .Where(s => s.Id == sharing.Id)
                        .FirstOrDefaultAsync();

                    if (trackableSharing != null)
                    {
                        trackableSharing.OwnerTokenSharing = ownerToken.tmpToken;
                        trackableSharing.GuestTokenSharing = guestToken.tmpToken;
                        trackableSharing.SessionExpiresAt = DateTimeOffset.UtcNow.AddHours(2);

                        _unitOfWork.GetGPSSharingRepository().Update(trackableSharing);
                        await _unitOfWork.SaveChangesAsync();

                        // Cập nhật local object để response đúng
                        sharing.SessionExpiresAt = trackableSharing.SessionExpiresAt;
                    }

                    ownerTokenValue = ownerToken.tmpToken!;
                    guestTokenValue = guestToken.tmpToken!;
                }
                else
                {
                    // Tokens còn hiệu lực → DÙNG LẠI
                    ownerTokenValue = sharing.OwnerTokenSharing!;
                    guestTokenValue = sharing.GuestTokenSharing!;
                }


                var response = new GPSSharingSessionResponse
                {
                    SessionId = sharing.Id,
                    InvitationCode = sharing.InvitationCode,
                    Status = sharing.Status,
                    SessionExpiresAt = sharing.SessionExpiresAt,
                    AcceptedAt = sharing.AcceptedAt,
                    AvatarOwner = ownerAvatar,
                    AvatarGuest = guestAvatar,
                    OwnerInfo = new ParticipantTrackingInfo
                    {
                        RenterId = sharing.OwnerBooking.RenterId,
                        RenterName = sharing.OwnerBooking.Renter.Account?.Username ?? "Unknown",
                        AvatarRenter = ownerAvatar,
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
                            tempTrackingPayload = new TempTrackingPayload
                            {
                                vehicleId = ownerVehicle.Id,
                                imei = ownerVehicle.GpsDeviceIdent ?? "",
                                deviceId = ownerVehicle.FlespiDeviceId,
                                exp = sharing.SessionExpiresAt!.Value.ToUnixTimeSeconds(),
                                tmpToken = ownerTokenValue
                            }
                        }
                    },

                    GuestInfo = new ParticipantTrackingInfo
                    {
                        RenterId = sharing.GuestBooking.RenterId,
                        RenterName = sharing.GuestBooking.Renter.Account?.Username ?? "Unknown",
                        AvatarRenter = guestAvatar,
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
                            tempTrackingPayload = new TempTrackingPayload
                            {
                                vehicleId = guestVehicle.Id,
                                imei = guestVehicle.GpsDeviceIdent ?? "",
                                deviceId = guestVehicle.FlespiDeviceId,
                                exp = sharing.SessionExpiresAt!.Value.ToUnixTimeSeconds(),
                                tmpToken = guestTokenValue
                            }
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



        public async Task<ResultResponse<List<GPSSharingHistoryResponse>>> GetSessionsByRenterId(
            Guid renterId)
        {
            try
            {
                
                var renter = await _unitOfWork.GetRenterRepository()
                    .FindByIdAsync(renterId);

                if (renter == null)
                    return ResultResponse<List<GPSSharingHistoryResponse>>.NotFound(
                        "Renter không tồn tại");

                
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