using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.MediaDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class MediaService:IMediaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFlespiService _flespiService;
    private readonly ICloudinaryService _cloudinaryService;


    public MediaService(IFlespiService protrackService, ICloudinaryService cloudinaryService, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _flespiService = protrackService;
        _cloudinaryService = cloudinaryService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<ResultResponse<MediaResponse>> AddMediaAsync(AddMediaRequest request)
    {
        if (request == null || request.File == null || request.File.Length == 0)
            return ResultResponse<MediaResponse>.Failure("No file provided.");

        try
        {
            var folderName = request.EntityType.ToString();

            string prefix = request.MediaType switch
            {
                MediaTypeEnum.Image => "img",
                MediaTypeEnum.Document => "doc",
                MediaTypeEnum.Video => "media",
                _ => "media"
            };

            string fileName = $"{prefix}_{Generator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}";

            var fileUrl = await _cloudinaryService.UploadImageFileAsync(
                request.File,
                fileName,
                folderName
            );

            if (string.IsNullOrEmpty(fileUrl))
                return ResultResponse<MediaResponse>.Failure("Failed to upload file.");

            var media = new Media
            {
                Id = Guid.NewGuid(),
                MediaType = request.MediaType.ToString(),
                FileUrl = fileUrl,
                DocNo = request.DocNo,
                EntityType = request.EntityType.ToString()
            };

            await _unitOfWork.GetMediaRepository().AddAsync(media);
            await _unitOfWork.SaveChangesAsync();

            var mediaResponse = new MediaResponse
            {
                Id = media.Id,
                MediaType = media.MediaType,
                FileUrl = media.FileUrl,
                DocNo = media.DocNo,
                EntityType = media.EntityType
            };

            return ResultResponse<MediaResponse>.SuccessResult("Media added successfully.", mediaResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<MediaResponse>.Failure($"An error occurred while adding media: {ex.Message}");
        }
    }

    public async Task<ResultResponse<MediaResponse>> UpdateSingleMediaAsync(MediaUpdateRequest updateRequest)
    {
        if (updateRequest == null || updateRequest.File == null || updateRequest.File.Length == 0)
            return ResultResponse<MediaResponse>.Failure("No file provided.");

        try
        {
            var existingMedia = await _unitOfWork.GetMediaRepository()
                .FindByIdAsync(updateRequest.MediaId);

            if (existingMedia == null)
                return ResultResponse<MediaResponse>.Failure("Media not found.");

           

            var folder = existingMedia.EntityType ?? "Other";

            string prefix = existingMedia.MediaType switch
            {
                "Image" => "img",
                "Document" => "doc",
                "Video" => "media",
                _ => "media"
            };

            string newFileName =
                $"{prefix}_{Generator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}";

            var newUrl = await _cloudinaryService.UploadImageFileAsync(
                updateRequest.File,
                newFileName,
                folder,
                existingMedia.FileUrl
            );

            if (!string.IsNullOrEmpty(newUrl))
                existingMedia.FileUrl = newUrl;

            _unitOfWork.GetMediaRepository().Update(existingMedia);
            await _unitOfWork.SaveChangesAsync();

            var mediaResponse = new MediaResponse
            {
                Id = existingMedia.Id,
                FileUrl = existingMedia.FileUrl,
                DocNo = existingMedia.DocNo,
                EntityType = existingMedia.EntityType
            };

            return ResultResponse<MediaResponse>
                .SuccessResult("Media updated successfully.", mediaResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<MediaResponse>.Failure(
                $"An error occurred while updating the media: {ex.Message}"
            );
        }
    }

}
