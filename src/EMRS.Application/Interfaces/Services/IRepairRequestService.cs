using EMRS.Application.Common;
using EMRS.Application.DTOs.RepairRequestDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services
{
    public interface IRepairRequestService
    {
        Task<ResultResponse<RepairRequestResponse>> UpdateRepairRequestTechnicianAsync(UpdateRepairRequestTechnician request);
        Task<ResultResponse<RepairRequestResponse>> CreateRepairRequestForTechnicianAsync(RepairRequestTechnicianCreateRequest request);
        Task<ResultResponse<PaginationResult<List<RepairRequestResponse>>>>
   GetByBranchIdAsync(int pageNum, int pageSize, bool orderByDesc);
        Task<ResultResponse<RepairRequestResponse>> CreateRepairRequestAsync(RepairRequestCreateRequest request);
        Task<ResultResponse<PaginationResult<List<RepairRequestResponse>>>> GetAllAsync(
   int pageNum, int pageSize, bool orderByDesc);
        Task<ResultResponse<RepairRequestResponse>> UpdateRepairRequestAsync(RepairRequestUpdateRequest request);
        Task<ResultResponse<RepairRequestDetailResponse>> GetByIdAsync(Guid id);
        Task<ResultResponse<PaginationResult<List<RepairRequestResponse>>>> GetByTechnicianIdAsync(
   Guid technicianId, int pageNum, int pageSize, bool orderByDesc);
    }
}
