using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.VehicleTransferDTOs;
using EMRS.Application.DTOs.VehicleTransferDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;

namespace EMRS.Application.Services
{
    public class VehicleTransferRequestService : IVehicleTransferRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public VehicleTransferRequestService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<ResultResponse<VehicleTransferRequestResponse>> CreateTransferRequest(
            VehicleTransferRequestCreateRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

     
                var userId = Guid.Parse(_currentUserService.UserId);
                var staff = await _unitOfWork.GetStaffRepository().GetStaffByAccountIdAsync(userId);

                if (staff == null)
                    return ResultResponse<VehicleTransferRequestResponse>.NotFound("Staff not found");

               
                if (staff.BranchId == null)
                    return ResultResponse<VehicleTransferRequestResponse>.Failure(
                        "Only branch managers can create transfer requests");

               
                var vehicleModel = await _unitOfWork.GetVehicleModelRepository()
                    .FindByIdAsync(request.VehicleModelId);
                if (vehicleModel == null)
                    return ResultResponse<VehicleTransferRequestResponse>.NotFound("Vehicle model not found");

                // Create transfer request
                var transferRequest = new VehicleTransferRequest
                {
                    VehicleModelId = request.VehicleModelId,
                    QuantityRequested = request.QuantityRequested,
                    Description = request.Description,
                    StaffId = staff.Id,
                    RequestedAt = DateTime.UtcNow,
                    Status = VehicleTransferRequestStatusEnum.Pending.ToString()
                };

                await _unitOfWork.GetVehicleTransferRequestRepository().AddAsync(transferRequest);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                
                var createdRequest = await _unitOfWork.GetVehicleTransferRequestRepository()
                    .GetRequestWithDetailsAsync(transferRequest.Id);

                var response = _mapper.Map<VehicleTransferRequestResponse>(createdRequest);
                return ResultResponse<VehicleTransferRequestResponse>.SuccessResult(
                    "Transfer request created successfully", response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<VehicleTransferRequestResponse>.Failure(
                    $"Error creating transfer request: {ex.Message}");
            }
        }

        public async Task<ResultResponse<List<VehicleTransferRequestResponse>>> GetAllPendingRequests()
        {
            try
            {
                var requests = await _unitOfWork.GetVehicleTransferRequestRepository()
                    .GetPendingRequestsAsync();

                var response = _mapper.Map<List<VehicleTransferRequestResponse>>(requests);
                return ResultResponse<List<VehicleTransferRequestResponse>>.SuccessResult(
                    "Pending transfer requests retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<VehicleTransferRequestResponse>>.Failure(
                    $"Error retrieving pending requests: {ex.Message}");
            }
        }

        public async Task<ResultResponse<List<VehicleTransferRequestResponse>>> GetAllRequests()
        {
            try
            {
                var requests = await _unitOfWork.GetVehicleTransferRequestRepository()
                    .GetAllRequestsWithDetailsAsync();

                var response = _mapper.Map<List<VehicleTransferRequestResponse>>(requests);
                return ResultResponse<List<VehicleTransferRequestResponse>>.SuccessResult(
                    "All transfer requests retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<VehicleTransferRequestResponse>>.Failure(
                    $"Error retrieving requests: {ex.Message}");
            }
        }

        public async Task<ResultResponse<List<VehicleTransferRequestResponse>>> GetRequestsByBranch(
            Guid branchId)
        {
            try
            {
                var requests = await _unitOfWork.GetVehicleTransferRequestRepository()
                    .GetRequestsByBranchAsync(branchId);

                var response = _mapper.Map<List<VehicleTransferRequestResponse>>(requests);
                return ResultResponse<List<VehicleTransferRequestResponse>>.SuccessResult(
     "Branch transfer requests retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<VehicleTransferRequestResponse>>.Failure(
                    $"Error retrieving branch requests: {ex.Message}");
            }
        }

        public async Task<ResultResponse<VehicleTransferRequestDetailResponse>> GetRequestDetail(
            Guid requestId)
        {
            try
            {
                var request = await _unitOfWork.GetVehicleTransferRequestRepository()
                    .GetRequestWithDetailsAsync(requestId);

                if (request == null)
                    return ResultResponse<VehicleTransferRequestDetailResponse>.NotFound(
                        "Transfer request not found");

                var response = _mapper.Map<VehicleTransferRequestDetailResponse>(request);
                return ResultResponse<VehicleTransferRequestDetailResponse>.SuccessResult(
                    "Transfer request detail retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<VehicleTransferRequestDetailResponse>.Failure(
                    $"Error retrieving request detail: {ex.Message}");
            }
        }

        public async Task<ResultResponse<VehicleTransferRequestResponse>> ApproveTransferRequest(
    Guid requestId)
{
    try
    {
        await _unitOfWork.BeginTransactionAsync();

        var request = await _unitOfWork.GetVehicleTransferRequestRepository()
            .FindByIdAsync(requestId);

        if (request == null)
            return ResultResponse<VehicleTransferRequestResponse>.NotFound(
                "Transfer request not found");

        if (request.Status != VehicleTransferRequestStatusEnum.Pending.ToString())
            return ResultResponse<VehicleTransferRequestResponse>.Failure(
                "Only pending requests can be approved");

        // Update request status
        request.Status = VehicleTransferRequestStatusEnum.Approved.ToString();
        request.ReviewedAt = DateTime.UtcNow;

        _unitOfWork.GetVehicleTransferRequestRepository().Update(request);
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitAsync();

        // Fetch full details for response
        var updatedRequest = await _unitOfWork.GetVehicleTransferRequestRepository()
            .GetRequestWithDetailsAsync(requestId);

        var response = _mapper.Map<VehicleTransferRequestResponse>(updatedRequest);
        return ResultResponse<VehicleTransferRequestResponse>.SuccessResult(
            "Transfer request approved successfully. Admin can now create transfer order.", response);
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackAsync();
        return ResultResponse<VehicleTransferRequestResponse>.Failure(
            $"Error approving request: {ex.Message}");
    }
}

public async Task<ResultResponse<VehicleTransferRequestResponse>> CancelTransferRequest(
    Guid requestId)
{
    try
    {
        await _unitOfWork.BeginTransactionAsync();

        var request = await _unitOfWork.GetVehicleTransferRequestRepository()
            .FindByIdAsync(requestId);

        if (request == null)
            return ResultResponse<VehicleTransferRequestResponse>.NotFound(
                "Transfer request not found");

        if (request.Status == VehicleTransferRequestStatusEnum.Cancelled.ToString())
            return ResultResponse<VehicleTransferRequestResponse>.Failure(
                "Request is already cancelled");

        // Update request status
        request.Status = VehicleTransferRequestStatusEnum.Cancelled.ToString();
        request.ReviewedAt = DateTime.UtcNow;

        _unitOfWork.GetVehicleTransferRequestRepository().Update(request);
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitAsync();

        // Fetch full details for response
        var updatedRequest = await _unitOfWork.GetVehicleTransferRequestRepository()
            .GetRequestWithDetailsAsync(requestId);

        var response = _mapper.Map<VehicleTransferRequestResponse>(updatedRequest);
        return ResultResponse<VehicleTransferRequestResponse>.SuccessResult(
            "Transfer request cancelled successfully", response);
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackAsync();
        return ResultResponse<VehicleTransferRequestResponse>.Failure(
            $"Error cancelling request: {ex.Message}");
    }
}
    }
}