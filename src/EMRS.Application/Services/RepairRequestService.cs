using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.RepairRequestDTOs;
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
    public class RepairRequestService:IRepairRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        public RepairRequestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResultResponse<RepairRequestResponse>> CreateRepairRequestAsync(RepairRequestCreateRequest request)
        {
            try
            {
                var newRepairRequest = new RepairRequest
                {
                    IssueDescription = request.IssueDescription,
                    Status = RepairStatus.Pending.ToString(),
                    VehicleId = request.VehicleId,
                    
                };
                var foundedVehicle = await _unitOfWork.GetVehicleRepository().FindByIdAsync(request.VehicleId);
                if (foundedVehicle == null)
                {
                    return ResultResponse<RepairRequestResponse>.NotFound("Vehicle not found.");
                }
                foundedVehicle.Status = VehicleStatusEnum.Repaired.ToString();
                await _unitOfWork.GetRepairRequestRepository().AddAsync(newRepairRequest);
                await _unitOfWork.SaveChangesAsync();
                var response = new RepairRequestResponse
                {
                    Id = newRepairRequest.Id,
                    VehicleId = newRepairRequest.VehicleId,
                    Priority = newRepairRequest.Priority,
                    Status = newRepairRequest.Status,
                    IssueDescription = newRepairRequest.IssueDescription,
                    ApprovedAt = newRepairRequest.ApprovedAt,
                    CreatedAt = newRepairRequest.CreatedAt,
                };
                return ResultResponse<RepairRequestResponse>.SuccessResult("Repair request created successfully.", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<RepairRequestResponse>.Failure($"Error creating repair request: {ex.Message}");
            }
        }
        public async Task<ResultResponse<PaginationResult<List<RepairRequestResponse>>>> GetAllAsync(
    int pageNum, int pageSize, bool orderByDesc)
        {
            try
            {
                var result = await _unitOfWork.GetRepairRequestRepository()
                    .GetAllPaginatedAsync(pageSize, pageNum, orderByDesc);

                var list = result.Items.Select(r => new RepairRequestResponse
                {
                    Id = r.Id,
                    VehicleId = r.VehicleId,
                    TechnicianId = r.TechnicianId,
                    Priority = r.Priority,
                    Status = r.Status,
                    IssueDescription = r.IssueDescription,
                    ApprovedAt = r.ApprovedAt,
                    CreatedAt = r.CreatedAt
                }).ToList();

                var response = new PaginationResult<List<RepairRequestResponse>>
                {
                    CurrentPage = result.CurrentPage,
                    PageSize = result.PageSize,
                    TotalPages = result.TotalPages,
                    TotalItems = result.TotalItems,
                    Items = list
                };

                return ResultResponse<PaginationResult<List<RepairRequestResponse>>>
                    .SuccessResult("Retrieved successfully.", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<PaginationResult<List<RepairRequestResponse>>>
                    .Failure($"Server error: {ex.Message}");
            }
        }

        public async Task<ResultResponse<PaginationResult<List<RepairRequestResponse>>>> GetByTechnicianIdAsync(
    Guid technicianId, int pageNum, int pageSize, bool orderByDesc)
        {
            try
            {
                var result = await _unitOfWork.GetRepairRequestRepository()
                    .GetByTechnicianIdPaginatedAsync(technicianId, pageSize, pageNum, orderByDesc);

                var list = result.Items.Select(r => new RepairRequestResponse
                {
                    Id = r.Id,
                    VehicleId = r.VehicleId,
                    TechnicianId = r.TechnicianId,
                    Priority = r.Priority,
                    Status = r.Status,
                    IssueDescription = r.IssueDescription,
                    ApprovedAt = r.ApprovedAt,
                    CreatedAt = r.CreatedAt
                }).ToList();

                var response = new PaginationResult<List<RepairRequestResponse>>
                {
                    CurrentPage = result.CurrentPage,
                    PageSize = result.PageSize,
                    TotalPages = result.TotalPages,
                    TotalItems = result.TotalItems,
                    Items = list
                };

                return ResultResponse<PaginationResult<List<RepairRequestResponse>>>
                    .SuccessResult("Retrieved successfully.", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<PaginationResult<List<RepairRequestResponse>>>
                    .Failure($"Server error: {ex.Message}");
            }
        }

        public async Task<ResultResponse<RepairRequestResponse>> GetByIdAsync(Guid id)
        {
            try
            {
                var rr = await _unitOfWork.GetRepairRequestRepository().FindByIdAsync(id);
                if (rr == null)
                    return ResultResponse<RepairRequestResponse>.NotFound("Repair request not found.");

                var response = new RepairRequestResponse
                {
                    Id = rr.Id,
                    VehicleId = rr.VehicleId,
                    TechnicianId = rr.TechnicianId,
                    Priority = rr.Priority,
                    Status = rr.Status,
                    IssueDescription = rr.IssueDescription,
                    ApprovedAt = rr.ApprovedAt,
                    CreatedAt = rr.CreatedAt
                };

                return ResultResponse<RepairRequestResponse>.SuccessResult("Retrieved successfully.", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<RepairRequestResponse>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<ResultResponse<RepairRequestResponse>> UpdateRepairRequestAsync(RepairRequestUpdateRequest request)
        {
            try
            {
                var foundedRequest = await _unitOfWork.GetRepairRequestRepository().FindByIdAsync(request.Id);
                foundedRequest.Priority = request.Priority;
                foundedRequest.Status = request.Status;
                foundedRequest.ApprovedAt = DateTime.Now;
                foundedRequest.TechnicianId = request.StaffId;   
               
                 _unitOfWork.GetRepairRequestRepository().Update(foundedRequest);
                await _unitOfWork.SaveChangesAsync();
                var response = new RepairRequestResponse
                {
                    Id = foundedRequest.Id,
                    TechnicianId = foundedRequest.TechnicianId.Value,
                    VehicleId = foundedRequest.VehicleId,
                    Priority = foundedRequest.Priority,
                    Status = foundedRequest.Status,
                    IssueDescription = foundedRequest.IssueDescription,
                    ApprovedAt = foundedRequest.ApprovedAt,
                    CreatedAt = foundedRequest.CreatedAt,
                };
                return ResultResponse<RepairRequestResponse>.SuccessResult("Repair request created successfully.", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<RepairRequestResponse>.Failure($"Error creating repair request: {ex.Message}");
            }
        }
    }
}
