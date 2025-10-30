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
    public class InsuranceClaimRepository : GenericRepository<InsuranceClaim>, IInsuranceClaimRepository
    {
        private readonly EMRSDbContext _dbContext;

        public InsuranceClaimRepository(EMRSDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<InsuranceClaim?> GetInsuranceClaimWithDetailsAsync(Guid id)
        {
            return await Query()
                .Include(ic => ic.Booking)
                    .ThenInclude(b => b.Vehicle)
                .Include(ic => ic.Booking)
                    .ThenInclude(b => b.VehicleModel)
                .Include(ic => ic.Renter)
                    .ThenInclude(r => r.Account)
                .Where(ic => ic.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<List<InsuranceClaim>> GetInsuranceClaimsByRenterIdAsync(Guid renterId)
        {
            return await Query()
                .Include(ic => ic.Booking)
                .Where(ic => ic.RenterId == renterId)
                .OrderByDescending(ic => ic.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<InsuranceClaim>> GetPendingInsuranceClaimsAsync()
        {
            return await Query()
                .Include(ic => ic.Booking)
                .Include(ic => ic.Renter)
                    .ThenInclude(r => r.Account)
                .Where(ic => ic.Status == "Reported")
                .OrderBy(ic => ic.CreatedAt)
                .ToListAsync();
        }
    }
}
