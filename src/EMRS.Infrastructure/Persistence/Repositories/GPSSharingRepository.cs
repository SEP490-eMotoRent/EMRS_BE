using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

       /* public async Task<GPSSharing?> GetActiveSessionByRenterIdAsync(Guid renterId)
        {
            return await Query()
                .Where(s => (s.OwnerRenterId == renterId || s.GuestRenterId == renterId)
                    && s.Status == GPSSharingStatusEnum.Active.ToString()
                    && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
        }*/

       /* public async Task<GPSSharing?> GetPendingInvitationByVehicleIdAsync(Guid vehicleId)
        {
            return await Query()
                .Where(s => s.OwnerVehicleId == vehicleId
                    && s.Status == GPSSharingStatusEnum.Pending.ToString()
                    && !s.IsDeleted)
                .FirstOrDefaultAsync();
        }*/

    /*    public async Task<List<GPSSharing>> GetSessionsByRenterIdAsync(Guid renterId)
        {
            return await Query()
                .Where(s => (s.OwnerRenterId == renterId || s.GuestRenterId == renterId)
                    && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }*/

        public async Task<List<GPSSharing>> GetAllSessionsForHistoryAsync()
        {
            return await Query()
                .Where(s => !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

      /*  public async Task<GPSSharing?> GetSessionWithDetailsAsync(Guid sessionId)
        {
            return await Query()
                .Include(s => s.OwnerRenter)
                    .ThenInclude(r => r.Account)
                .Include(s => s.OwnerVehicle)
                    .ThenInclude(v => v.VehicleModel)
                .Include(s => s.GuestRenter)
                    .ThenInclude(r => r.Account)
                .Include(s => s.GuestVehicle)
                    .ThenInclude(v => v.VehicleModel)
                .Where(s => s.Id == sessionId && !s.IsDeleted)
                .FirstOrDefaultAsync();
        }*/
    }
}
