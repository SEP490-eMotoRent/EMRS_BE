// EMRS.Infrastructure/Persistence/Repositories/GPSSharingRepository.cs
using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EMRS.Infrastructure.Persistence.Repositories
{
    public class GPSSharingRepository : GenericRepository<GPSSharing>, IGPSSharingRepository
    {
        private readonly EMRSDbContext _dbContext;

        public GPSSharingRepository(EMRSDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GPSSharing?> GetByInvitationCodeAsync(string invitationCode)
        {
            return await Query()
                .Where(s => s.InvitationCode == invitationCode && !s.IsDeleted)
                .FirstOrDefaultAsync();
        }

       
        public async Task<GPSSharing?> GetActiveSessionByBookingIdAsync(Guid bookingId)
        {
            return await Query()
                .Where(s => (s.OwnerBookingId == bookingId || s.GuestBookingId == bookingId)
                    && s.Status == GPSSharingStatusEnum.Active.ToString()
                    && !s.IsDeleted)
                .FirstOrDefaultAsync();
        }

        
        public async Task<GPSSharing?> GetActiveSessionByRenterIdAsync(Guid renterId)
        {
            return await Query()
                .Include(s => s.OwnerBooking)
                .Include(s => s.GuestBooking)
                .Where(s => (s.OwnerBooking.RenterId == renterId || s.GuestBooking!.RenterId == renterId)
                    && s.Status == GPSSharingStatusEnum.Active.ToString()
                    && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
        }

        
        public async Task<GPSSharing?> GetPendingInvitationByBookingIdAsync(Guid bookingId)
        {
            return await Query()
                .Where(s => s.OwnerBookingId == bookingId
                    && s.Status == GPSSharingStatusEnum.Pending.ToString()
                    && !s.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<GPSSharing>> GetSessionsByRenterIdAsync(Guid renterId)
        {
            return await Query()
                .Include(s => s.OwnerBooking)
                .Include(s => s.GuestBooking)
                .Where(s => (s.OwnerBooking.RenterId == renterId 
                        || (s.GuestBooking != null && s.GuestBooking.RenterId == renterId))
                    && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<GPSSharing>> GetAllSessionsForHistoryAsync()
        {
            return await Query()
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
                .Where(s => !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<GPSSharing?> GetSessionWithDetailsAsync(Guid sessionId)
        {
            return await Query().AsNoTracking()
                .Include(s => s.OwnerBooking)
                    .ThenInclude(b => b.Renter)
                        .ThenInclude(r => r.Account)
                .Include(s => s.OwnerBooking)
                    .ThenInclude(b => b.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                .Include(s => s.GuestBooking)
                    .ThenInclude(b => b.Renter)
                        .ThenInclude(r => r.Account)
                .Include(s => s.GuestBooking)
                    .ThenInclude(b => b.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                .Where(s => s.Id == sessionId && !s.IsDeleted)
                .FirstOrDefaultAsync();
        }
    }
}