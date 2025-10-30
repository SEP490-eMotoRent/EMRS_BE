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
    public class InsurancePackageRepository : GenericRepository<InsurancePackage>, IInsurancePackageRepository
    {
        private readonly EMRSDbContext _dbContext;

        public InsurancePackageRepository(EMRSDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<InsurancePackage>> GetAllActivePackagesAsync()
        {
            return await Query()
                .Where(ip => ip.IsActive && !ip.IsDeleted)
                .OrderBy(ip => ip.PackageFee)
                .ToListAsync();
        }

        public async Task<InsurancePackage?> GetPackageByNameAsync(string packageName)
        {
            return await Query()
                .Where(ip => ip.PackageName == packageName && !ip.IsDeleted)
                .FirstOrDefaultAsync();
        }
    }
}
