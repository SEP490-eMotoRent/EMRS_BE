using EMRS.Application.Common;
using EMRS.Application.DTOs.BranchDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services;

public interface IBranchService
{
    Task<ResultResponse<BranchResponse>> GetBranchByIdAsync(Guid branchId);
    Task<ResultResponse<BranchResponse>> CreateABranch(CreateBranchRequest createBranchRequest);
    Task<ResultResponse<List<BranchModelDetailResponse>>> GetAllBranchesWithSameModelIdAsync(Guid vehicleModelId);
    Task<ResultResponse<List<BranchResponse>>> GetNearbyBranchesAsync(
     double lat, double lon, double radiusKm);
    Task<ResultResponse<List<BranchResponse>>> GetAllBranches();
    Task<ResultResponse<List<BranchSearchListResponse>>>
        SearchWithTimeSpanForBranch(BranchSearchRequest branchSearchRequest);

    Task<ResultResponse<BranchResponse>> UpdateBranch(Guid branchId, UpdateBranchRequest updateBranchRequest);
    Task<ResultResponse<bool>> DeleteBranch(Guid branchId);
}
