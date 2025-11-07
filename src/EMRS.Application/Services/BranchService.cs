using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.VehicleModelDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
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
    public async Task<ResultResponse<List<BranchResponse>>> GetAllBranches()
    {
        try
        {
            var branches = await _unitOfWork.GetBranchRepository().GetAllAsync();
            var branchResponses = _mapper.Map<List<BranchResponse>>(branches);
            return ResultResponse<List<BranchResponse>>.SuccessResult("Branches retrieved successfully.", branchResponses);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<BranchResponse>>.Failure("An error occurred while retrieving branches: " + ex.Message);
        }
    }
    public async Task<ResultResponse<List<BranchSearchListResponse>>>
        SearchWithTimeSpanForBranch(BranchSearchRequest branchSearchRequest)
    {
        try
        {
            var branch = await _unitOfWork.GetBranchRepository()
                .SearchBranchWithAvailableModelsAsync(branchSearchRequest);

            var vehicleModelsIds = branch.Select(v => v.Id).ToList();
            
            var listresponse = branch.Select(v =>
            {
                return new BranchSearchListResponse
                {
                 Id = v.Id,
                 Address = v.Address,
                 BranchName = v.BranchName,
                 City = v.City,
                 ClosingTime = v.ClosingTime,
                 Email = v.Email,
                 Latitude = v.Latitude,
                 Longitude = v.Longitude,
                 OpeningTime = v.OpeningTime,
                 Phone= v.Phone,
                };
            }).ToList();
           
            return ResultResponse<List<BranchSearchListResponse>>.SuccessResult("Branches retrieved successfully.", listresponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<BranchSearchListResponse>>.Failure($"An error occurred while retrieving branches: {ex.Message}");
        }
    }
}
