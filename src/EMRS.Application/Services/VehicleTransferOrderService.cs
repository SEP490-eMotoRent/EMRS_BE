using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.VehicleTransferDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services
{
    public class VehicleTransferOrderService : IVehicleTransferOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public VehicleTransferOrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<ResultResponse<VehicleTransferOrderResponse>> CreateTransferOrder(
            VehicleTransferOrderCreateRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validate vehicle exists
                var vehicle = await _unitOfWork.GetVehicleRepository()
                    .FindByIdAsync(request.VehicleId);
                if (vehicle == null)
                    return ResultResponse<VehicleTransferOrderResponse>.NotFound("Vehicle not found");

                // Validate vehicle is at from branch
                if (vehicle.BranchId != request.FromBranchId)
                    return ResultResponse<VehicleTransferOrderResponse>.Failure(
                        "Vehicle is not currently at the specified from branch");

                // Validate vehicle is available
                if (vehicle.Status != VehicleStatusEnum.Available.ToString())
                    return ResultResponse<VehicleTransferOrderResponse>.Failure(
                        $"Vehicle is not available for transfer. Current status: {vehicle.Status}");

                // Validate branches exist
                var fromBranch = await _unitOfWork.GetBranchRepository()
                    .FindByIdAsync(request.FromBranchId);
                if (fromBranch == null)
                    return ResultResponse<VehicleTransferOrderResponse>.NotFound("From branch not found");

                var toBranch = await _unitOfWork.GetBranchRepository()
                    .FindByIdAsync(request.ToBranchId);
                if (toBranch == null)
                    return ResultResponse<VehicleTransferOrderResponse>.NotFound("To branch not found");

                // Validate not transferring to same branch
                if (request.FromBranchId == request.ToBranchId)
                    return ResultResponse<VehicleTransferOrderResponse>.Failure(
                        "Cannot transfer vehicle to the same branch");

                // Create transfer order
                var transferOrder = new VehicleTransferOrder
                {
                    VehicleId = request.VehicleId,
                    FromBranchId = request.FromBranchId,
                    ToBranchId = request.ToBranchId,
                    Notes = request.Notes,
                    Status = VehicleTransferOrderStatusEnum.Pending.ToString()
                };

                // Update vehicle status to Transferring and lock it
                vehicle.Status = VehicleStatusEnum.Transfering.ToString();

                await _unitOfWork.GetVehicleTransferOrderRepository().AddAsync(transferOrder);
                _unitOfWork.GetVehicleRepository().Update(vehicle);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                // Fetch full details for response
                var createdOrder = await _unitOfWork.GetVehicleTransferOrderRepository()
                    .GetOrderWithDetailsAsync(transferOrder.Id);

                var response = _mapper.Map<VehicleTransferOrderResponse>(createdOrder);
                return ResultResponse<VehicleTransferOrderResponse>.SuccessResult(
                    "Transfer order created successfully. Vehicle is now locked and awaiting dispatch.",
                    response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<VehicleTransferOrderResponse>.Failure(
                    $"Error creating transfer order: {ex.Message}");
            }
        }

        public async Task<ResultResponse<VehicleTransferOrderResponse>> ConfirmVehicleDispatched(
            Guid orderId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Get order with details
                var order = await _unitOfWork.GetVehicleTransferOrderRepository()
                    .GetOrderWithDetailsAsync(orderId);

                if (order == null)
                    return ResultResponse<VehicleTransferOrderResponse>.NotFound(
                        "Transfer order not found");

                if (order.Status != VehicleTransferOrderStatusEnum.Pending.ToString())
                    return ResultResponse<VehicleTransferOrderResponse>.Failure(
                        $"Only pending orders can be dispatched. Current status: {order.Status}");

                // Validate current user is from the source branch
                var userId = Guid.Parse(_currentUserService.UserId);
                var staff = await _unitOfWork.GetStaffRepository().GetStaffByAccountIdAsync(userId);

                if (staff == null || staff.BranchId != order.FromBranchId)
                    return ResultResponse<VehicleTransferOrderResponse>.Forbidden(
                        "Only managers from the source branch can confirm dispatch");

                // Update order status to InTransit
                order.Status = VehicleTransferOrderStatusEnum.InTransit.ToString();

                _unitOfWork.GetVehicleTransferOrderRepository().Update(order);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                // Fetch updated details
                var updatedOrder = await _unitOfWork.GetVehicleTransferOrderRepository()
                    .GetOrderWithDetailsAsync(orderId);

                var response = _mapper.Map<VehicleTransferOrderResponse>(updatedOrder);
                return ResultResponse<VehicleTransferOrderResponse>.SuccessResult(
                    "Vehicle dispatched successfully. Vehicle is now in transit.", response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<VehicleTransferOrderResponse>.Failure(
                    $"Error confirming dispatch: {ex.Message}");
            }
        }

        public async Task<ResultResponse<VehicleTransferOrderResponse>> ConfirmVehicleReceived(
    Guid orderId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Get order with details
                var order = await _unitOfWork.GetVehicleTransferOrderRepository()
                    .GetOrderWithDetailsAsync(orderId);

                if (order == null)
                    return ResultResponse<VehicleTransferOrderResponse>.NotFound(
                        "Transfer order not found");

                if (order.Status != VehicleTransferOrderStatusEnum.InTransit.ToString())
                    return ResultResponse<VehicleTransferOrderResponse>.Failure(
                        $"Only in-transit orders can be received. Current status: {order.Status}");

                // Validate current user is from the destination branch
                var userId = Guid.Parse(_currentUserService.UserId);
                var staff = await _unitOfWork.GetStaffRepository().GetStaffByAccountIdAsync(userId);

                if (staff == null || staff.BranchId != order.ToBranchId)
                    return ResultResponse<VehicleTransferOrderResponse>.Forbidden(
                        "Only managers from the destination branch can confirm receipt");

                // ⭐ FIX: Use vehicle from order (already loaded with tracking)
                var vehicle = order.Vehicle;

                if (vehicle == null)
                    return ResultResponse<VehicleTransferOrderResponse>.NotFound("Vehicle not found");

                // CRITICAL: Update vehicle branch to destination branch
                vehicle.BranchId = order.ToBranchId;

                // CRITICAL: Change vehicle status back to Available
                vehicle.Status = VehicleStatusEnum.Available.ToString();

                // Update order status to Completed
                order.Status = VehicleTransferOrderStatusEnum.Completed.ToString();
                order.ReceivedDate = DateTime.UtcNow;

                // ⭐ FIX: No need to call Update() - EF tracks changes automatically
                // Just save changes
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                // ⭐ FIX: Map from current order object (already has all data)
                var response = _mapper.Map<VehicleTransferOrderResponse>(order);

                return ResultResponse<VehicleTransferOrderResponse>.SuccessResult(
                    "Vehicle received successfully. Vehicle is now available at the destination branch.",
                    response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<VehicleTransferOrderResponse>.Failure(
                    $"Error confirming receipt: {ex.Message}");
            }
        }
        public async Task<ResultResponse<VehicleTransferOrderResponse>> CancelOrder(Guid orderId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var order = await _unitOfWork.GetVehicleTransferOrderRepository()
                    .GetOrderWithDetailsAsync(orderId);

                if (order == null)
                    return ResultResponse<VehicleTransferOrderResponse>.NotFound(
                        "Transfer order not found");

                // Can only cancel pending orders
                if (order.Status != VehicleTransferOrderStatusEnum.Pending.ToString())
                    return ResultResponse<VehicleTransferOrderResponse>.Failure(
                        "Only pending orders can be cancelled. Vehicle may already be in transit.");

                // Get vehicle
                var vehicle = await _unitOfWork.GetVehicleRepository()
                    .FindByIdAsync(order.VehicleId);

                if (vehicle == null)
                    return ResultResponse<VehicleTransferOrderResponse>.NotFound("Vehicle not found");

                // Unlock vehicle - change status back to Available
                vehicle.Status = VehicleStatusEnum.Available.ToString();

                // Update order status to Cancelled
                order.Status = VehicleTransferOrderStatusEnum.Cancelled.ToString();

                _unitOfWork.GetVehicleTransferOrderRepository().Update(order);
                _unitOfWork.GetVehicleRepository().Update(vehicle);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                // Fetch updated details
                var updatedOrder = await _unitOfWork.GetVehicleTransferOrderRepository()
                    .GetOrderWithDetailsAsync(orderId);

                var response = _mapper.Map<VehicleTransferOrderResponse>(updatedOrder);
                return ResultResponse<VehicleTransferOrderResponse>.SuccessResult(
                    "Transfer order cancelled successfully. Vehicle is now available again.", response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<VehicleTransferOrderResponse>.Failure(
                    $"Error cancelling order: {ex.Message}");
            }
        }

        public async Task<ResultResponse<List<VehicleTransferOrderResponse>>> GetAllOrders()
        {
            try
            {
                var orders = await _unitOfWork.GetVehicleTransferOrderRepository()
                    .GetAllOrdersWithDetailsAsync();

                var response = _mapper.Map<List<VehicleTransferOrderResponse>>(orders);
                return ResultResponse<List<VehicleTransferOrderResponse>>.SuccessResult(
                    "All transfer orders retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<VehicleTransferOrderResponse>>.Failure(
                    $"Error retrieving orders: {ex.Message}");
            }
        }

        public async Task<ResultResponse<List<VehicleTransferOrderResponse>>> GetOrdersByBranch(
            Guid branchId)
        {
            try
            {
                var ordersFrom = await _unitOfWork.GetVehicleTransferOrderRepository()
                    .GetOrdersByFromBranchAsync(branchId);
                var ordersTo = await _unitOfWork.GetVehicleTransferOrderRepository()
                    .GetOrdersByToBranchAsync(branchId);

                // Combine and remove duplicates
                var allOrders = ordersFrom.Concat(ordersTo)
                    .GroupBy(o => o.Id)
                    .Select(g => g.First())
                    .OrderByDescending(o => o.CreatedAt)
                    .ToList();

                var response = _mapper.Map<List<VehicleTransferOrderResponse>>(allOrders);
                return ResultResponse<List<VehicleTransferOrderResponse>>.SuccessResult(
                    "Branch transfer orders retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<VehicleTransferOrderResponse>>.Failure(
                    $"Error retrieving branch orders: {ex.Message}");
            }
        }

        public async Task<ResultResponse<List<VehicleTransferOrderResponse>>> GetPendingOrdersByBranch(
            Guid branchId)
        {
            try
            {
                var orders = await _unitOfWork.GetVehicleTransferOrderRepository()
                    .GetPendingOrdersByBranchAsync(branchId);

                var response = _mapper.Map<List<VehicleTransferOrderResponse>>(orders);
                return ResultResponse<List<VehicleTransferOrderResponse>>.SuccessResult(
                    "Pending branch transfer orders retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<VehicleTransferOrderResponse>>.Failure(
                    $"Error retrieving pending orders: {ex.Message}");
            }
        }

        public async Task<ResultResponse<List<VehicleTransferOrderResponse>>> GetInTransitOrders()
        {
            try
            {
                var orders = await _unitOfWork.GetVehicleTransferOrderRepository()
                    .GetInTransitOrdersAsync();

                var response = _mapper.Map<List<VehicleTransferOrderResponse>>(orders);
                return ResultResponse<List<VehicleTransferOrderResponse>>.SuccessResult(
                    "In-transit orders retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<VehicleTransferOrderResponse>>.Failure(
                    $"Error retrieving in-transit orders: {ex.Message}");
            }
        }

        public async Task<ResultResponse<VehicleTransferOrderDetailResponse>> GetOrderDetail(
            Guid orderId)
        {
            try
            {
                var order = await _unitOfWork.GetVehicleTransferOrderRepository()
                    .GetOrderWithDetailsAsync(orderId);

                if (order == null)
                    return ResultResponse<VehicleTransferOrderDetailResponse>.NotFound(
                        "Transfer order not found");

                var response = _mapper.Map<VehicleTransferOrderDetailResponse>(order);
                return ResultResponse<VehicleTransferOrderDetailResponse>.SuccessResult(
                    "Transfer order detail retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<VehicleTransferOrderDetailResponse>.Failure(
                    $"Error retrieving order detail: {ex.Message}");
            }
        }

    }
}
