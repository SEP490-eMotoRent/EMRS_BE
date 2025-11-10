using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
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
      /*  public async Task<IEnumerable<Feedback>> GetFeedbackByBookingIdAsync(Guid bookingId)
        {
            return await Query().Where(f => f.BookingId == bookingId)
                .AsEnumerable();
        }*/
    }
}
