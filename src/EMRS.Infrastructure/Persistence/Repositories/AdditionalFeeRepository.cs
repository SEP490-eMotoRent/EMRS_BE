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
    public class AdditionalFeeRepository: GenericRepository<AdditionalFee>, IAdditionalFeeRepository
    {
        private readonly EMRSDbContext _dbContext;
        public AdditionalFeeRepository(EMRSDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<AdditionalFee>> GetAdditionalFeesByBookingIdAsync(Guid bookingId)
        {
            return await Query()
                .Where(af => af.BookingId == bookingId)
                .OrderBy(af => af.CreatedAt)
                .ToListAsync();
        }

    }
}
