using EMRS.Application.Common;
using EMRS.Application.DTOs.VehicleTransferDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services
{
    public interface IVehicleTransferOrderService
    {
        // Admin creates transfer order (selects specific vehicle)
        Task<ResultResponse<VehicleTransferOrderResponse>> CreateTransferOrder(
            VehicleTransferOrderCreateRequest request);

        // Manager (from branch) confirms vehicle dispatched
        Task<ResultResponse<VehicleTransferOrderResponse>> ConfirmVehicleDispatched(Guid orderId);

        // Manager (to branch) confirms vehicle received
        Task<ResultResponse<VehicleTransferOrderResponse>> ConfirmVehicleReceived(Guid orderId);

        // Get all transfer orders
        Task<ResultResponse<List<VehicleTransferOrderResponse>>> GetAllOrders();

        // Get transfer orders by branch (from or to)
        Task<ResultResponse<List<VehicleTransferOrderResponse>>> GetOrdersByBranch(Guid branchId);

        // Get pending orders for a specific branch
        Task<ResultResponse<List<VehicleTransferOrderResponse>>> GetPendingOrdersByBranch(Guid branchId);

        // Get all in-transit orders
        Task<ResultResponse<List<VehicleTransferOrderResponse>>> GetInTransitOrders();

        // Get order detail
        Task<ResultResponse<VehicleTransferOrderDetailResponse>> GetOrderDetail(Guid orderId);

        // Cancel order (Admin only, before dispatched)
        Task<ResultResponse<VehicleTransferOrderResponse>> CancelOrder(Guid orderId);
    }
}
