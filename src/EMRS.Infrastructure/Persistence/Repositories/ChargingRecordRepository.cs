using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence.Repositories
{
    public class ChargingRecordRepository : GenericRepository<ChargingRecord>, IChargingRecordRepository
    {
        private readonly EMRSDbContext _dbContext;

        public ChargingRecordRepository(EMRSDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ChargingRecord?> GetLastChargingRecordByBookingIdAsync(Guid bookingId)
        {
            return await Query()
                .Where(cr => cr.BookingId == bookingId)
                .OrderByDescending(cr => cr.ChargingDate)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ChargingRecord>> GetChargingRecordsByBookingIdAsync(Guid bookingId)
        {
            return await Query()
                .Where(cr => cr.BookingId == bookingId)
                .OrderByDescending(cr => cr.ChargingDate)
                .ToListAsync();
        }

        public async Task<List<ChargingRecord>> GetChargingRecordsByRenterIdAsync(Guid renterId)
        {
            return await Query()
                .Include(cr => cr.Booking)
                    .ThenInclude(b => b.Vehicle)
                    .ThenInclude(v => v.VehicleModel)
                .Include(cr => cr.Booking)
                    .ThenInclude(b => b.Vehicle)
                .Include(cr => cr.Branch)
                .Include(cr => cr.Staff)
                    .ThenInclude(s => s.Account)
                .Where(cr => cr.Booking.RenterId == renterId)
                .OrderByDescending(cr => cr.ChargingDate)
                .ToListAsync();
        }
    }
}
