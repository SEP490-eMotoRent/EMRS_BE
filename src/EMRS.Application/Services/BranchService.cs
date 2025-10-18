using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class BranchService:IBranchService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IMapper _mapper;   
    public BranchService(IUnitOfWork unitOfWork,IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<ResultResponse<BranchResponse>> CreateABranch(CreateBranchRequest createBranchRequest)
    {
        try
        {
            var branch = new Branch
            {
                Address = createBranchRequest.Address,
                BranchName = createBranchRequest.BranchName,
                City = createBranchRequest.City,
                Email = createBranchRequest.Email,
                Phone = createBranchRequest.Phone,
                Latitude = createBranchRequest.Latitude,
                Longitude = createBranchRequest.Longitude,
                OpeningTime = createBranchRequest.OpeningTime,
                ClosingTime = createBranchRequest.ClosingTime
            };
            await _unitOfWork.GetBranchRepository().AddAsync(branch);
            await _unitOfWork.SaveChangesAsync();
            BranchResponse branchResponse = _mapper.Map<BranchResponse>(branch);
            return ResultResponse<BranchResponse>.SuccessResult("Branch created successfully.", branchResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<BranchResponse>.Failure("An error occurred while creating the branch: " + ex.Message);
        }
        


    }
}
