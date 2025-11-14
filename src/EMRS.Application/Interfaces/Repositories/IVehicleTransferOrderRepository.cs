using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories
{
    public interface IVehicleTransferOrderRepository : IGenericRepository<VehicleTransferOrder>
    {
        Task<VehicleTransferOrder?> GetOrderWithDetailsAsync(Guid id);
        Task<List<VehicleTransferOrder>> GetAllOrdersWithDetailsAsync();
        Task<List<VehicleTransferOrder>> GetOrdersByFromBranchAsync(Guid branchId);
        Task<List<VehicleTransferOrder>> GetOrdersByToBranchAsync(Guid branchId);
        Task<List<VehicleTransferOrder>> GetPendingOrdersByBranchAsync(Guid branchId);
        Task<List<VehicleTransferOrder>> GetInTransitOrdersAsync();
    }
}
