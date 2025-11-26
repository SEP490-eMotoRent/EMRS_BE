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
            var vnTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            var vnNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTz);
            var todayVN = DateOnly.FromDateTime(vnNow);

            return await Query()
                .FirstOrDefaultAsync(a =>
                    a.HolidayDate == todayVN &&
                    a.IsActive &&
                    !a.IsDeleted);
        }



    }
}
