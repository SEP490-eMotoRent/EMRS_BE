using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using EMRS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMRS.Infrastructure.Persistence.Repositories
{
    public class VehicleTransferRequestRepository : GenericRepository<VehicleTransferRequest>,
        IVehicleTransferRequestRepository
    {
        private readonly EMRSDbContext _dbContext;

        public VehicleTransferRequestRepository(EMRSDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<VehicleTransferRequest?> GetRequestWithDetailsAsync(Guid id)
        {
            return await Query()
                .Include(r => r.VehicleModel)
                .Include(r => r.Staff)
                    .ThenInclude(s => s.Account)
                .Include(r => r.Staff)
                    .ThenInclude(s => s.Branch)
                .Include(r => r.VehicleTransferOrder)
                    .ThenInclude(o => o.Vehicle)
                .Include(r => r.VehicleTransferOrder)
                    .ThenInclude(o => o.FromBranch)
                .Include(r => r.VehicleTransferOrder)
                    .ThenInclude(o => o.ToBranch)
                .Where(r => r.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<List<VehicleTransferRequest>> GetAllRequestsWithDetailsAsync()
        {
            return await Query()
                .Include(r => r.VehicleModel)
                .Include(r => r.Staff)
                    .ThenInclude(s => s.Account)
                .Include(r => r.Staff)
                    .ThenInclude(s => s.Branch)
                .Include(r => r.VehicleTransferOrder)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<VehicleTransferRequest>> GetRequestsByBranchAsync(Guid branchId)
        {
            return await Query()
                .Include(r => r.VehicleModel)
                .Include(r => r.Staff)
                    .ThenInclude(s => s.Account)
                .Include(r => r.Staff)
                    .ThenInclude(s => s.Branch)
                .Where(r => r.Staff.BranchId == branchId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<VehicleTransferRequest>> GetPendingRequestsAsync()
        {
            return await Query()
                .Include(r => r.VehicleModel)
                .Include(r => r.Staff)
                    .ThenInclude(s => s.Account)
                .Include(r => r.Staff)
                    .ThenInclude(s => s.Branch)
                .Where(r => r.Status == "Pending")
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}