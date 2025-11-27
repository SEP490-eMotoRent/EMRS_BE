using AutoMapper;
using EMRS.Application.Abstractions;
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
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services
{
    public class GPSSharingService : IGPSSharingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFlespiService _flespiService;

        public GPSSharingService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IFlespiService flespiService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _flespiService = flespiService;
        }

        // ============================================
        // CREATE INVITATION (OWNER)
        // ============================================
        public async Task<ResultResponse<GPSSharingInviteResponse>> CreateInvitation(
            GPSSharingCreateRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var accountId = Guid.Parse(_currentUserService.UserId);

                // 1. Lấy Renter từ AccountId
                var renter = await _unitOfWork.GetRenterRepository()
                    .Query()
                    .Where(r => r.Id == accountId)
                    .FirstOrDefaultAsync();

                if (renter == null)
                    return ResultResponse<GPSSharingInviteResponse>.NotFound(
                        "Không tìm thấy thông tin khách thuê");

                //// 2. Validate: Renter đang có booking ACTIVE với xe này
                //var activeBooking = await _unitOfWork.GetBookingRepository()
                //    .Query()
                //    .Where(b => b.RenterId == renter.Id
                //        && b.VehicleId == request.VehicleId
                //        && b.BookingStatus == BookingStatusEnum.Renting.ToString())
                //    .FirstOrDefaultAsync();

                //if (activeBooking == null)
                //    return ResultResponse<GPSSharingInviteResponse>.Failure(
                //        "Bạn không đang thuê xe này");

                // 3. Check: Xe này đã có invitation Pending chưa
                var existingInvitation = await _unitOfWork.GetGPSSharingRepository()
                    .GetPendingInvitationByVehicleIdAsync(request.VehicleId);

                if (existingInvitation != null)
                    return ResultResponse<GPSSharingInviteResponse>.Failure(
                        "Xe này đã có lời mời đang chờ. Vui lòng hủy hoặc đợi hết hạn.");

                // 4. Check: Owner đã có session Active chưa
                var activeSession = await _unitOfWork.GetGPSSharingRepository()
                    .GetActiveSessionByRenterIdAsync(renter.Id);

                if (activeSession != null)
                    return ResultResponse<GPSSharingInviteResponse>.Failure(
                        "Bạn đang có session sharing đang hoạt động");

                // 5. Generate unique invitation code (6 ký tự)
                var invitationCode = await GenerateUniqueInvitationCode();

                // 6. Tạo GPSSharing record
                var sharing = new GPSSharing
                {
                    InvitationCode = invitationCode,
                    Status = GPSSharingStatusEnum.Pending.ToString(),
                    ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30), // 30 phút
                    OwnerRenterId = renter.Id,
                    OwnerVehicleId = request.VehicleId
                    // SessionExpiresAt sẽ set khi Guest join
                };

                await _unitOfWork.GetGPSSharingRepository().AddAsync(sharing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                // 7. Lấy vehicle info cho response
                var vehicle = await _unitOfWork.GetVehicleRepository()
                    .FindByIdAsync(request.VehicleId);

                var response = new GPSSharingInviteResponse
                {
                    SessionId = sharing.Id,
                    InvitationCode = invitationCode,
                    InvitationExpiresAt = sharing.ExpiresAt,
                    OwnerVehicleLicensePlate = vehicle?.LicensePlate ?? "Unknown"
                };

                return ResultResponse<GPSSharingInviteResponse>.SuccessResult(
                    "Lời mời đã tạo thành công", response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<GPSSharingInviteResponse>.Failure(
                    $"Lỗi khi tạo lời mời: {ex.Message}");
            }
        }

        // ============================================
        // JOIN SHARING (GUEST)
        // ============================================
        public async Task<ResultResponse<GPSSharingActiveResponse>> JoinSharing(
            GPSSharingJoinRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var accountId = Guid.Parse(_currentUserService.UserId);

                // 1. Lấy Renter từ AccountId
                var guestRenter = await _unitOfWork.GetRenterRepository()
                    .Query()
                    .Where(r => r.Id == accountId)
                    .FirstOrDefaultAsync();

                if (guestRenter == null)
                    return ResultResponse<GPSSharingActiveResponse>.NotFound(
                        "Không tìm thấy thông tin khách thuê");

                // 2. Tìm invitation
                var sharing = await _unitOfWork.GetGPSSharingRepository()
                    .GetByInvitationCodeAsync(request.InvitationCode);

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
                if (guestRenter.Id == sharing.OwnerRenterId)
                    return ResultResponse<GPSSharingActiveResponse>.Failure(
                        "Bạn không thể tham gia session của chính mình");

                //// 6. Validate: Guest đang có booking ACTIVE với xe
                //var guestBooking = await _unitOfWork.GetBookingRepository()
                //    .Query()
                //    .Where(b => b.RenterId == guestRenter.Id
                //        && b.VehicleId == request.GuestVehicleId
                //        && b.BookingStatus == BookingStatusEnum.Renting.ToString())
                //    .FirstOrDefaultAsync();

                //if (guestBooking == null)
                //    return ResultResponse<GPSSharingActiveResponse>.Failure(
                //        "Bạn không đang thuê xe này");

                // 7. Check: Guest đã có session Active chưa
                var existingGuestSession = await _unitOfWork.GetGPSSharingRepository()
                    .GetActiveSessionByRenterIdAsync(guestRenter.Id);

                if (existingGuestSession != null)
                    return ResultResponse<GPSSharingActiveResponse>.Failure(
                        "Bạn đang có session sharing khác đang hoạt động");

                // 8. Update sharing record
                sharing.GuestRenterId = guestRenter.Id;
                sharing.GuestVehicleId = request.GuestVehicleId;
                sharing.Status = GPSSharingStatusEnum.Active.ToString();
                sharing.AcceptedAt = DateTimeOffset.UtcNow;
                sharing.SessionExpiresAt = DateTimeOffset.UtcNow.AddHours(24); // 24 giờ

                _unitOfWork.GetGPSSharingRepository().Update(sharing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                // 9. Tạo tokens cho CẢ 2 xe (dùng FlespiService)
                var ownerVehicle = await _unitOfWork.GetVehicleRepository()
                    .FindByIdAsync(sharing.OwnerVehicleId);
                var guestVehicle = await _unitOfWork.GetVehicleRepository()
                    .FindByIdAsync(request.GuestVehicleId);

                if (ownerVehicle == null || guestVehicle == null)
                    return ResultResponse<GPSSharingActiveResponse>.Failure(
                        "Không tìm thấy thông tin xe");

                // Tạo token với TTL = 24 giờ
                var ownerToken = await _flespiService.CreateFlespiAclTokenAsync(
                    ownerVehicle, ttlSeconds: 86400, minutes: 1440); // 24h
                var guestToken = await _flespiService.CreateFlespiAclTokenAsync(
                    guestVehicle, ttlSeconds: 86400, minutes: 1440); // 24h

                // 10. Lấy thông tin Owner
                var ownerRenter = await _unitOfWork.GetRenterRepository()
                    .Query()
                    .Include(r => r.Account)
                    .Where(r => r.Id == sharing.OwnerRenterId)
                    .FirstOrDefaultAsync();

                // 11. Build response
                var response = new GPSSharingActiveResponse
                {
                    SessionId = sharing.Id,
                    SessionExpiresAt = sharing.SessionExpiresAt.Value,

                    OwnerInfo = new ParticipantTrackingInfo
                    {
                        RenterId = sharing.OwnerRenterId,
                        RenterName = ownerRenter?.Account?.Username ?? "Unknown",
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
                        RenterId = guestRenter.Id,
                        RenterName = guestRenter.Account?.Username ?? _currentUserService.Username,
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
                    "Đã kết nối thành công", response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<GPSSharingActiveResponse>.Failure(
                    $"Lỗi khi tham gia session: {ex.Message}");
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
                    .FindByIdAsync(sessionId);

                if (sharing == null)
                    return ResultResponse<GPSSharingSessionResponse>.NotFound(
                        "Session không tồn tại");

                // Validate quyền truy cập
                if (sharing.OwnerRenterId != renter.Id && sharing.GuestRenterId != renter.Id)
                    return ResultResponse<GPSSharingSessionResponse>.Forbidden(
                        "Bạn không có quyền truy cập session này");

                // Nếu session đã kết thúc → Không trả token
                if (sharing.Status != GPSSharingStatusEnum.Active.ToString())
                {
                    var ownerRenter = await _unitOfWork.GetRenterRepository()
                        .Query()
                        .Include(r => r.Account)
                        .Where(r => r.Id == sharing.OwnerRenterId)
                        .FirstOrDefaultAsync();
                    var ownerVehicle = await _unitOfWork.GetVehicleRepository()
                        .FindByIdAsync(sharing.OwnerVehicleId);

                    Renter? guestRenter = null;
                    Vehicle? guestVehicle = null;
                    if (sharing.GuestRenterId.HasValue && sharing.GuestVehicleId.HasValue)
                    {
                        guestRenter = await _unitOfWork.GetRenterRepository()
                            .Query()
                            .Include(r => r.Account)
                            .Where(r => r.Id == sharing.GuestRenterId.Value)
                            .FirstOrDefaultAsync();
                        guestVehicle = await _unitOfWork.GetVehicleRepository()
                            .FindByIdAsync(sharing.GuestVehicleId.Value);
                    }

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
                                RenterId = sharing.OwnerRenterId,
                                RenterName = ownerRenter?.Account?.Username ?? "Unknown",
                                Vehicle = new VehicleTrackingResponse
                                {
                                    Id = ownerVehicle.Id,
                                    LicensePlate = ownerVehicle.LicensePlate,
                                    Color = ownerVehicle.Color,
                                    Status = ownerVehicle.Status
                                    // Không trả token
                                }
                            },

                            GuestInfo = guestRenter != null && guestVehicle != null ? new ParticipantTrackingInfo
                            {
                                RenterId = sharing.GuestRenterId!.Value,
                                RenterName = guestRenter.Account?.Username ?? "Unknown",
                                Vehicle = new VehicleTrackingResponse
                                {
                                    Id = guestVehicle.Id,
                                    LicensePlate = guestVehicle.LicensePlate,
                                    Color = guestVehicle.Color,
                                    Status = guestVehicle.Status
                                }
                            } : null
                        });
                }

                // Session Active → Trả token
                var ownerVeh = await _unitOfWork.GetVehicleRepository()
                    .FindByIdAsync(sharing.OwnerVehicleId);
                var guestVeh = await _unitOfWork.GetVehicleRepository()
                    .FindByIdAsync(sharing.GuestVehicleId!.Value);

                var ownerToken = await _flespiService.CreateFlespiAclTokenAsync(
                    ownerVeh, ttlSeconds: 86400, minutes: 1440);
                var guestToken = await _flespiService.CreateFlespiAclTokenAsync(
                    guestVeh, ttlSeconds: 86400, minutes: 1440);

                var ownerRent = await _unitOfWork.GetRenterRepository()
                    .Query()
                    .Include(r => r.Account)
                    .Where(r => r.Id == sharing.OwnerRenterId)
                    .FirstOrDefaultAsync();
                var guestRent = await _unitOfWork.GetRenterRepository()
                    .Query()
                    .Include(r => r.Account)
                    .Where(r => r.Id == sharing.GuestRenterId!.Value)
                    .FirstOrDefaultAsync();

                var response = new GPSSharingSessionResponse
                {
                    SessionId = sharing.Id,
                    InvitationCode = sharing.InvitationCode,
                    Status = sharing.Status,
                    SessionExpiresAt = sharing.SessionExpiresAt,
                    AcceptedAt = sharing.AcceptedAt,

                    OwnerInfo = new ParticipantTrackingInfo
                    {
                        RenterId = sharing.OwnerRenterId,
                        RenterName = ownerRent?.Account?.Username ?? "Unknown",
                        Vehicle = new VehicleTrackingResponse
                        {
                            Id = ownerVeh.Id,
                            LicensePlate = ownerVeh.LicensePlate,
                            Color = ownerVeh.Color,
                            YearOfManufacture = ownerVeh.YearOfManufacture,
                            CurrentOdometerKm = ownerVeh.CurrentOdometerKm,
                            BatteryHealthPercentage = ownerVeh.BatteryHealthPercentage,
                            Status = ownerVeh.Status,
                            PurchaseDate = ownerVeh.PurchaseDate,
                            Description = ownerVeh.Description,
                            tempTrackingPayload = ownerToken
                        }
                    },

                    GuestInfo = new ParticipantTrackingInfo
                    {
                        RenterId = sharing.GuestRenterId!.Value,
                        RenterName = guestRent?.Account?.Username ?? "Unknown",
                        Vehicle = new VehicleTrackingResponse
                        {
                            Id = guestVeh.Id,
                            LicensePlate = guestVeh.LicensePlate,
                            Color = guestVeh.Color,
                            YearOfManufacture = guestVeh.YearOfManufacture,
                            CurrentOdometerKm = guestVeh.CurrentOdometerKm,
                            BatteryHealthPercentage = guestVeh.BatteryHealthPercentage,
                            Status = guestVeh.Status,
                            PurchaseDate = guestVeh.PurchaseDate,
                            Description = guestVeh.Description,
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
                    .FindByIdAsync(sessionId);

                if (sharing == null)
                    return ResultResponse<bool>.NotFound("Session không tồn tại");

                // Validate quyền: Chỉ Owner hoặc Guest mới cancel được
                if (sharing.OwnerRenterId != renter.Id && sharing.GuestRenterId != renter.Id)
                    return ResultResponse<bool>.Forbidden(
                        "Bạn không có quyền hủy session này");

                // Chỉ cancel được nếu đang Pending hoặc Active
                if (sharing.Status != GPSSharingStatusEnum.Pending.ToString()
                    && sharing.Status != GPSSharingStatusEnum.Active.ToString())
                {
                    return ResultResponse<bool>.Failure(
                        "Session đã kết thúc, không thể hủy");
                }

                // Update status
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
        // GET ALL SESSIONS (FOR MANAGER/ADMIN)
        // ============================================
        public async Task<ResultResponse<List<GPSSharingHistoryResponse>>> GetAllSessions()
        {
            try
            {
                var sessions = await _unitOfWork.GetGPSSharingRepository()
                    .GetAllSessionsForHistoryAsync();

                var response = new List<GPSSharingHistoryResponse>();

                foreach (var session in sessions)
                {
                    var ownerRenter = await _unitOfWork.GetRenterRepository()
                        .Query()
                        .Include(r => r.Account)
                        .Where(r => r.Id == session.OwnerRenterId)
                        .FirstOrDefaultAsync();
                    var ownerVehicle = await _unitOfWork.GetVehicleRepository()
                        .FindByIdAsync(session.OwnerVehicleId);

                    Renter? guestRenter = null;
                    Vehicle? guestVehicle = null;
                    if (session.GuestRenterId.HasValue && session.GuestVehicleId.HasValue)
                    {
                        guestRenter = await _unitOfWork.GetRenterRepository()
                            .Query()
                            .Include(r => r.Account)
                            .Where(r => r.Id == session.GuestRenterId.Value)
                            .FirstOrDefaultAsync();
                        guestVehicle = await _unitOfWork.GetVehicleRepository()
                            .FindByIdAsync(session.GuestVehicleId.Value);
                    }

                    response.Add(new GPSSharingHistoryResponse
                    {
                        SessionId = session.Id,
                        InvitationCode = session.InvitationCode,
                        Status = session.Status,
                        CreatedAt = session.CreatedAt,
                        AcceptedAt = session.AcceptedAt,
                        SessionExpiresAt = session.SessionExpiresAt,

                        OwnerRenterId = session.OwnerRenterId,
                        OwnerRenterName = ownerRenter?.Account?.Username ?? "Unknown",
                        OwnerVehicleLicensePlate = ownerVehicle?.LicensePlate ?? "Unknown",

                        GuestRenterId = session.GuestRenterId,
                        GuestRenterName = guestRenter?.Account?.Username,
                        GuestVehicleLicensePlate = guestVehicle?.LicensePlate
                    });
                }

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
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Bỏ O,0,I,1
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
