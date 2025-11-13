using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using EMRS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMRS.Infrastructure.Persistence.Repositories
{
    public class VehicleTransferOrderRepository : GenericRepository<VehicleTransferOrder>,
        IVehicleTransferOrderRepository
    {
        private readonly EMRSDbContext _dbContext;

        public VehicleTransferOrderRepository(EMRSDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<VehicleTransferOrder?> GetOrderWithDetailsAsync(Guid id)
        {
            return await Query()
                .Include(o => o.Vehicle)
                    .ThenInclude(v => v.VehicleModel)
                .Include(o => o.FromBranch)
                .Include(o => o.ToBranch)
                .Include(o => o.VehicleTransferRequests)
                    .ThenInclude(r => r.Staff)
                        .ThenInclude(s => s.Account)
                .Include(o => o.VehicleTransferRequests)
                    .ThenInclude(r => r.VehicleModel)
                .Where(o => o.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<List<VehicleTransferOrder>> GetAllOrdersWithDetailsAsync()
        {
            return await Query()
                .Include(o => o.Vehicle)
                    .ThenInclude(v => v.VehicleModel)
                .Include(o => o.FromBranch)
                .Include(o => o.ToBranch)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<VehicleTransferOrder>> GetOrdersByFromBranchAsync(Guid branchId)
        {
            return await Query()
                .Include(o => o.Vehicle)
                    .ThenInclude(v => v.VehicleModel)
                .Include(o => o.FromBranch)
                .Include(o => o.ToBranch)
                .Where(o => o.FromBranchId == branchId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<VehicleTransferOrder>> GetOrdersByToBranchAsync(Guid branchId)
        {
            return await Query()
                .Include(o => o.Vehicle)
                    .ThenInclude(v => v.VehicleModel)
                .Include(o => o.FromBranch)
                .Include(o => o.ToBranch)
                .Where(o => o.ToBranchId == branchId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<VehicleTransferOrder>> GetPendingOrdersByBranchAsync(Guid branchId)
        {
            return await Query()
                .Include(o => o.Vehicle)
                    .ThenInclude(v => v.VehicleModel)
                .Include(o => o.FromBranch)
                .Include(o => o.ToBranch)
                .Where(o => (o.FromBranchId == branchId || o.ToBranchId == branchId)
                    && o.Status == "Pending")
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<VehicleTransferOrder>> GetInTransitOrdersAsync()
        {
            return await Query()
                .Include(o => o.Vehicle)
                    .ThenInclude(v => v.VehicleModel)
                .Include(o => o.FromBranch)
                .Include(o => o.ToBranch)
                .Where(o => o.Status == "InTransit")
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}