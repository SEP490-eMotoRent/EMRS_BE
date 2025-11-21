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
    public class HolidayPricingRepository: GenericRepository<HolidayPricing>,IHolidayPricingRepository
    {
        private readonly EMRSDbContext _dbContext;

        public HolidayPricingRepository(EMRSDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HolidayPricing?> GetHolidayByCurrentDateAsync()
        {
            var vnTime = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

            var today = DateTime.SpecifyKind(vnTime.Date, DateTimeKind.Utc);
            var tomorrow = today.AddDays(1);

            return await Query()
                .FirstOrDefaultAsync(a =>
                    a.HolidayDate >= today &&
                    a.HolidayDate < tomorrow &&
                    a.IsActive &&
                    !a.IsDeleted);
        }


    }
}
