using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.ConfigurationDTOs;
using EMRS.Application.DTOs.DocumentDTOs;
using EMRS.Application.Helper;
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

public class ConfigurationService:IConfigurationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFacePlusPlusService _facePlusClient;
    private readonly ICloudinaryService _cloudinaryService;

    public ConfigurationService(ICloudinaryService cloudinaryService,IFacePlusPlusService facePlusPlusClient, IUnitOfWork unitOfWork)
    {
        _cloudinaryService = cloudinaryService;
        _unitOfWork = unitOfWork;
        _facePlusClient = facePlusPlusClient;
    }

    public async Task<ResultResponse<Configuration>> CreateAsync(ConfigurationCreateRequest entity)
    {
        try
        {
            Configuration configuration = new Configuration
            {
                Description = entity.Description,
                Type = (int)entity.Type,
                Title = entity.Title,
                Value = entity.Value,
            };
            await _unitOfWork.GetConfigurationRepository().AddAsync(configuration);
            await _unitOfWork.SaveChangesAsync();
            return ResultResponse<Configuration>.SuccessResult("Configuration created successfully", configuration);
        }
        catch (Exception ex)
        {
            return ResultResponse<Configuration>.Failure("An error occurred while creating: " + ex.Message);
        }
    }
    public async Task<ResultResponse<Configuration>> CreateConfigurationWithMediaAsync(ConfigurationMediaCreateRequest request)
    {
        try
        {
            string newFileName =
             $"doc_{Generator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            var stringUrl = await _cloudinaryService.UploadDocumentFileAsync
                (
                request.File,
                newFileName,
                "Configuration"
                );
            Configuration configuration = new Configuration
            {
                Description= request.Description,
                Type = (int)request.Type,
                Title = request.Title,
                Value = stringUrl,
            };
            await _unitOfWork.GetConfigurationRepository().AddAsync(configuration);
            await _unitOfWork.SaveChangesAsync();
            return ResultResponse<Configuration>.SuccessResult("Configuration created successfully", configuration);
        }
        catch (Exception ex)
        {
            return ResultResponse<Configuration>.Failure("An error occurred while creating: " + ex.Message);
        }
    }
    public async Task<ResultResponse<Configuration>> UpdateConfigWithMediaAsync(ConfigurationMediaUpdateRequest entity)
    {
        try
        {
            string newFileName =
            $"doc_{Generator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            var stringUrl = await _cloudinaryService.UploadDocumentFileAsync
                (
                entity.File,
                newFileName,
                "Configuration"
                );
            if(string.IsNullOrEmpty(stringUrl))
                return ResultResponse<Configuration>.Failure("File upload failed");
            var existing = await _unitOfWork.GetConfigurationRepository().FindByIdAsync(entity.Id);
            if (existing == null||existing.IsDeleted)
                return ResultResponse<Configuration>.NotFound("Configuration not found");
            existing.Title = entity.Title;
            existing.Description = entity.Description;
            existing.Type = (int)entity.Type;
            existing.Value = stringUrl;
            _unitOfWork.GetConfigurationRepository().Update(existing);
            await _unitOfWork.SaveChangesAsync();

            return ResultResponse<Configuration>.SuccessResult("Configuration updated successfully", existing);
        }
        catch (Exception ex)
        {
            return ResultResponse<Configuration>.Failure("An error occurred while updating: " + ex.Message);
        }
    }
    public async Task<ResultResponse<Configuration?>> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _unitOfWork.GetConfigurationRepository().FindByIdAsync(id);
            if (entity == null || entity.IsDeleted)
                return ResultResponse<Configuration>.NotFound("Configuration not found");

            return ResultResponse<Configuration>.SuccessResult("Configuration fetched successfully", entity);
        }
        catch (Exception ex)
        {
            return ResultResponse<Configuration>.Failure("An error occurred while fetching: " + ex.Message);
        }
    }

    public async Task<ResultResponse<List<Configuration>>> GetAllAsync()
    {
        try
        {
            var list = await _unitOfWork.GetConfigurationRepository().GetAllAsync();
            return ResultResponse<Configuration>.SuccessList("Configurations fetched successfully", list);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<Configuration>>.Failure("An error occurred while fetching: " + ex.Message);
        }
    }
    public async Task<ResultResponse<List<Configuration>>> GetByTypeAsync(ConfigurationTypeEnum type)
    {
        try
        {
            var list = await _unitOfWork.GetConfigurationRepository()
                .Query()
                .Where(c => c.Type == (int)type)
                .ToListAsync(); 

            if (!list.Any())
                return ResultResponse<List<Configuration>>.NotFound("No configurations found for this type");

            return ResultResponse<List<Configuration>>.SuccessResult("Configurations fetched successfully", list);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<Configuration>>.Failure("An error occurred while fetching: " + ex.Message);
        }
    }

    public async Task<ResultResponse<Configuration>> UpdateAsync(Configuration entity)
    {
        try
        {
            var existing = await _unitOfWork.GetConfigurationRepository().FindByIdAsync(entity.Id);
            if (existing == null|| existing.IsDeleted)
                return ResultResponse<Configuration>.NotFound("Configuration not found");
            existing.Title = entity.Title;
            existing.Description = entity.Description;
            existing.Type = entity.Type;
            existing.Value = entity.Value;

            _unitOfWork.GetConfigurationRepository().Update(existing);
            await _unitOfWork.SaveChangesAsync();

            return ResultResponse<Configuration>.SuccessResult("Configuration updated successfully", entity);
        }
        catch (Exception ex)
        {
            return ResultResponse<Configuration>.Failure("An error occurred while updating: " + ex.Message);
        }
    }

    public async Task<ResultResponse<object>> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _unitOfWork.GetConfigurationRepository().FindByIdAsync(id);
            if (entity == null || entity.IsDeleted)
                return ResultResponse<object>.NotFound("Configuration not found");

            _unitOfWork.GetConfigurationRepository().Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            return ResultResponse<object>.SuccessResult("Configuration deleted successfully", new object());
        }
        catch (Exception ex)
        {
            return ResultResponse<object>.Failure("An error occurred while deleting: " + ex.Message);
        }
    }

    public async Task<ResultResponse<string>> RemoveFaceSet(string facesettoken)
    {
        try
        {
            bool? result = await _facePlusClient.DeleteFaceSetAsync(facesettoken);
            if (result == false)
                return ResultResponse<string>.Failure("Failed to delete FaceSet");

            return ResultResponse<string>.SuccessResult("FaceSet deleted successfully", null);
        }
        catch (Exception ex)
        {
            return ResultResponse<string>.Failure("An error occurred while deleting FaceSet: " + ex.Message);
        }
    }

    public async Task<ResultResponse<string>> CreateFaceSet()
    {
        try
        {
            string? faceSetToken = await _facePlusClient.CreateFaceSetAsync();
            if (string.IsNullOrEmpty(faceSetToken))
                return ResultResponse<string>.Failure("Failed to create FaceSet");

            return ResultResponse<string>.SuccessResult("FaceSet created successfully", faceSetToken);
        }
        catch (Exception ex)
        {
            return ResultResponse<string>.Failure("An error occurred while creating FaceSet: " + ex.Message);
        }
    }

    public async Task<ResultResponse<List<Configuration>>> GetAdditionalPricingConfigurationsAsync()
    {
        try
        {
            var configs = await _unitOfWork.GetConfigurationRepository()
                .Query()
                .Where(c => c.Type == (int)ConfigurationTypeEnum.LateReturnPrice ||
                           c.Type == (int)ConfigurationTypeEnum.CleaningPrice ||
                           c.Type == (int)ConfigurationTypeEnum.CrossBranchPrice ||
                           c.Type == (int)ConfigurationTypeEnum.EconomyExcessKmPrice ||
                           c.Type == (int)ConfigurationTypeEnum.StandardExcessKmPrice ||
                           c.Type == (int)ConfigurationTypeEnum.PreniumExcessKmPrice ||
                           c.Type == (int)ConfigurationTypeEnum.EconomyExcessKmLimit ||
                           c.Type == (int)ConfigurationTypeEnum.StandardExcessKmLimit ||
                           c.Type == (int)ConfigurationTypeEnum.PreniumExcessKmLimit)
                .OrderBy(c => c.Type)
                .ToListAsync();

            if (!configs.Any())
                return ResultResponse<List<Configuration>>.NotFound("Pricing configurations not found");

            return ResultResponse<List<Configuration>>.SuccessResult(
                "Pricing configurations fetched successfully", configs);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<Configuration>>.Failure(
                "An error occurred while fetching pricing configurations: " + ex.Message);
        }
    }

}
