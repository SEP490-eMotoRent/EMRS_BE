using EMRS.Application.Common;
using EMRS.Application.DTOs.VehicleTransferDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services
{
    public interface IVehicleTransferRequestService
    {
        // Manager creates request
        Task<ResultResponse<VehicleTransferRequestResponse>> CreateTransferRequest(
            VehicleTransferRequestCreateRequest request);

        // Admin views all pending requests
        Task<ResultResponse<List<VehicleTransferRequestResponse>>> GetAllPendingRequests();

        // Admin/Manager view all requests
        Task<ResultResponse<List<VehicleTransferRequestResponse>>> GetAllRequests();

        // Manager views requests from their branch
        Task<ResultResponse<List<VehicleTransferRequestResponse>>> GetRequestsByBranch(Guid branchId);

        // Get request detail
        Task<ResultResponse<VehicleTransferRequestDetailResponse>> GetRequestDetail(Guid requestId);

        // Admin approves request (will create VehicleTransferOrder separately)
        Task<ResultResponse<VehicleTransferRequestResponse>> ApproveTransferRequest(Guid requestId);

        // Admin/Manager cancels request
        Task<ResultResponse<VehicleTransferRequestResponse>> CancelTransferRequest(Guid requestId);
    }
}
