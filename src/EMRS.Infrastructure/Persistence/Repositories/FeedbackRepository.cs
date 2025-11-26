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
    public class FeedbackRepository:GenericRepository<Feedback>, IFeedbackRepository
    {
        private readonly EMRSDbContext _context;
        public FeedbackRepository(EMRSDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<List<Feedback>> GetFeedbackByBookingIdAsync(Guid bookingId)
        {
            return await Query().Include(v=>v.Renter.Account)
                .Where(f => f.BookingId == bookingId)
                .ToListAsync()??new List<Feedback>();
        }
        public async Task<List<Feedback>> GetFeedbacksAsync()
        {
            return await Query().Include(v => v.Renter.Account)
                .ToListAsync() ?? new List<Feedback>();
        }
        public async Task<List<Feedback>> GetFeedbackByVehicleModelIdAsync(Guid vehicleModelId)
        {
            return await Query()
                .Include(v => v.Renter.Account)
                .Where(f => f.Booking.VehicleModelId == vehicleModelId)
                .ToListAsync();
        }

    }
}
