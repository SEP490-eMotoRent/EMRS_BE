using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.BranchDTOs;
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
        private readonly ICurrentUserService _currentUserService;
        public RepairRequestService(IUnitOfWork unitOfWork,ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
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
        public async Task<ResultResponse<RepairRequestResponse>> CreateRepairRequestForTechnicianAsync(RepairRequestTechnicianCreateRequest request)
        {
            try
            {
                var technicianId = _currentUserService.UserId;
                if (technicianId == null)
                {
                    return ResultResponse<RepairRequestResponse>.Unauthorized("You have not yet logged in.");

                }
                var newRepairRequest = new RepairRequest
                {
                    IssueDescription = request.IssueDescription,
                    Status = RepairStatus.Completed.ToString(),
                    VehicleId = request.VehicleId,
                    Priority= request.Priority,
                    TechnicianId = Guid.Parse(technicianId),
                    ApprovedAt= request.ApprovedAt,
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
        public async Task<ResultResponse<PaginationResult<List<RepairRequestResponse>>>>
    GetByBranchIdAsync(int pageNum, int pageSize, bool orderByDesc)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(userId == null) {
                    return ResultResponse<PaginationResult<List<RepairRequestResponse>>>.Unauthorized("User not authorized.");
                }
                var branch = await _unitOfWork.GetBranchRepository().GetBranchByStaffIdAsync(Guid.Parse(userId));
               
                var data = await _unitOfWork.GetRepairRequestRepository()
                    .GetByBranchIdPaginatedAsync(branch.Id, pageSize, pageNum, orderByDesc);

                var mapped = data.Items.Select(r => new RepairRequestResponse
                {
                    Id = r.Id,
                    IssueDescription = r.IssueDescription,
                    Priority = r.Priority,
                    Status = r.Status,
                    ApprovedAt = r.ApprovedAt,
                    VehicleId = r.VehicleId,
                    TechnicianId = r.TechnicianId
                }).ToList();

                return ResultResponse<PaginationResult<List<RepairRequestResponse>>>.SuccessResult(
                    "Fetched successfully",
                    new PaginationResult<List<RepairRequestResponse>>
                    {
                        CurrentPage = data.CurrentPage,
                        PageSize = data.PageSize,
                        TotalItems = data.TotalItems,
                        TotalPages = data.TotalPages,
                        Items = mapped
                    }
                );
            }
            catch (Exception ex)
            {
                return ResultResponse<PaginationResult<List<RepairRequestResponse>>>.Failure(ex.Message);
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

        public async Task<ResultResponse<RepairRequestDetailResponse>> GetByIdAsync(Guid id)
        {
            try
            {
                var rr = await _unitOfWork.GetRepairRequestRepository().GetRepairRequestWithReferencesAsync(id);
                if (rr == null)
                    return ResultResponse<RepairRequestDetailResponse>.NotFound("Repair request not found.");
                
                
               
                var response = new RepairRequestDetailResponse
                {
                    branch  = rr.Vehicle.Branch==null?null: new BranchResponse
                    {
                        Address = rr.Vehicle.Branch.Address,
                        BranchName = rr.Vehicle.Branch.BranchName,
                        City = rr.Vehicle.Branch.City,
                        ClosingTime = rr.Vehicle.Branch.ClosingTime,
                        Email = rr.Vehicle.Branch.Email,
                        Id = rr.Vehicle.Branch.Id,
                        Latitude = rr.Vehicle.Branch.Latitude,
                        Longitude = rr.Vehicle.Branch.Longitude,
                        OpeningTime = rr.Vehicle.Branch.OpeningTime,
                        Phone = rr.Vehicle.Branch.Phone,
                    },
                    Id = rr.Id,
                    VehicleId = rr.VehicleId,
                    TechnicianId = rr.TechnicianId,
                    Priority = rr.Priority,
                    Status = rr.Status,
                    IssueDescription = rr.IssueDescription,
                    ApprovedAt = rr.ApprovedAt,
                    CreatedAt = rr.CreatedAt
                };

                return ResultResponse<RepairRequestDetailResponse>.SuccessResult("Retrieved successfully.", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<RepairRequestDetailResponse>.Failure($"Error: {ex.Message}");
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
        public async Task<ResultResponse<RepairRequestResponse>> UpdateRepairRequestTechnicianAsync(Guid  requestId)
        {
            try
            {
                var foundedRequest = await _unitOfWork.GetRepairRequestRepository().FindByIdAsync(requestId);
                foundedRequest.Status = RepairStatus.Completed.ToString();

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
