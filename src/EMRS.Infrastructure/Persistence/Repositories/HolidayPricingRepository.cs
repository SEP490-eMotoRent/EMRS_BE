using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence.Repositories
{
    public class HolidayPricingRepository: GenericRepository<HolidayPricing>,IHolidayPricingRepository
    {
        private readonly EMRSDbContext _dbContext;

        public HolidayPricingRepository(EMRSDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
