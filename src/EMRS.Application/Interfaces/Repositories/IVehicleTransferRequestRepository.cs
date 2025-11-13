using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories
{
    public interface IVehicleTransferRequestRepository
    {
        // Custom methods (CRUD inherited from IGenericRepository)
        Task<VehicleTransferRequest?> GetRequestWithDetailsAsync(Guid id);
        Task<List<VehicleTransferRequest>> GetAllRequestsWithDetailsAsync();
        Task<List<VehicleTransferRequest>> GetRequestsByBranchAsync(Guid branchId);
        Task<List<VehicleTransferRequest>> GetPendingRequestsAsync();
    }
}
