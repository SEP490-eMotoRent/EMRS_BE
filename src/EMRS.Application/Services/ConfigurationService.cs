using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.ConfigurationDTOs;
using EMRS.Application.DTOs.DocumentDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class ConfigurationService:IConfigurationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFacePlusPlusClient _facePlusClient;
    public ConfigurationService(IFacePlusPlusClient facePlusPlusClient,IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _facePlusClient = facePlusPlusClient;
    }
    public async Task<ResultResponse<Configuration>> CreateAsync(ConfigurationCreateRequest entity)
    {
        Configuration configuration = new Configuration
        {
            Description = entity.Description,
            Type = (int)entity.Type,
            Title = entity.Title,
            Value = entity.Value,
        };
        await _unitOfWork.GetConfigurationRepository().AddAsync(configuration);
        await _unitOfWork.CommitAsync();
        return ResultResponse<Configuration>.SuccessResult("Configuration created successfully", configuration);
    }

    public async Task<ResultResponse<Configuration?>> GetByIdAsync(Guid id)
    {
        var entity = await _unitOfWork.GetConfigurationRepository().FindByIdAsync(id);
        if (entity == null)
            return ResultResponse<Configuration>.NotFound("Configuration not found");

        return ResultResponse<Configuration>.SuccessResult("Configuration fetched successfully", entity);
    }

    public async Task<ResultResponse<List<Configuration>>> GetAllAsync()
    {
        var list = await _unitOfWork.GetConfigurationRepository().GetAllAsync();
        return ResultResponse<Configuration>.SuccessList("Configurations fetched successfully", list);
    }

    public async Task<ResultResponse<Configuration>> UpdateAsync(Configuration entity)
    {
        var existing = await _unitOfWork.GetConfigurationRepository().FindByIdAsync(entity.Id);
        if (existing == null)
            return ResultResponse<Configuration>.NotFound("Configuration not found");

        _unitOfWork.GetConfigurationRepository().Update(entity);
        await _unitOfWork.CommitAsync();
        return ResultResponse<Configuration>.SuccessResult("Configuration updated successfully", entity);
    }

    public async Task<ResultResponse<object>> DeleteAsync(Guid id)
    {
        var entity = await _unitOfWork.GetConfigurationRepository().FindByIdAsync(id);
        if (entity == null)
            return ResultResponse<object>.NotFound("Configuration not found");

        _unitOfWork.GetConfigurationRepository().Delete(entity);
        await _unitOfWork.CommitAsync();
        return ResultResponse<object>.SuccessResult("Configuration deleted successfully", new object());
    }
    public async Task<ResultResponse<string>> RemoveFaceSet(string facesettoken)
    {
        try
        {
           bool? result= await _facePlusClient.DeleteFaceSetAsync(facesettoken);
            if (result==false)
                return ResultResponse<string>.Failure("Failed to create FaceSet");
            return ResultResponse<string>.SuccessResult("FaceSetDeleteSuccess",null);

        }
        catch (Exception ex)
        {
            return ResultResponse<string>.Failure("An error occurred while creating the branch: " + ex.Message);

        }
    }
    public async Task<ResultResponse<string>> CreateFaceSet()
    {
        try
        {
            string? faceSetToken = await _facePlusClient.CreateFaceSetAsync();
            if (string.IsNullOrEmpty(faceSetToken))
                return ResultResponse<string>.Failure("Failed to create FaceSet");
            return ResultResponse<string>.SuccessResult("FaceSetDeleteSuccess", faceSetToken);

        }
        catch (Exception ex)
        {
            return ResultResponse<string>.Failure("An error occurred while creating the branch: " + ex.Message);

        }
    }
}
