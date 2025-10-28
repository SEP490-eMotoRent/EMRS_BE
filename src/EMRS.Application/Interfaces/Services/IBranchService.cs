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
    Task<ResultResponse<BranchResponse>> CreateABranch(CreateBranchRequest createBranchRequest);

    Task<ResultResponse<List<BranchResponse>>> GetAllBranches();
}
