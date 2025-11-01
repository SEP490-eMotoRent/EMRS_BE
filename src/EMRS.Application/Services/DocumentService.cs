using EMRS.Application.Abstractions;
using EMRS.Application.Abstractions.Models;
using EMRS.Application.Common;
using EMRS.Application.DTOs.AccountDTOs;
using EMRS.Application.DTOs.BookingDTOs;
using EMRS.Application.DTOs.DocumentDTOs;
using EMRS.Application.DTOs.InsuranceClaimDTOs;
using EMRS.Application.DTOs.RenterDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class DocumentService:IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFacePlusPlusClient _facePlusPlusClient;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ICurrentUserService _currentUserService;

    public DocumentService(
        ICurrentUserService currentUserService,
        ICloudinaryService cloudinaryService,
        IFacePlusPlusClient facePlusPlusClient,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _cloudinaryService = cloudinaryService;
        _unitOfWork = unitOfWork;
        _facePlusPlusClient = facePlusPlusClient;
    }
    public async Task<ResultResponse<string>> DeleteDocumentAsync(Guid documentId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

          
            Document document= await _unitOfWork.GetDocumentRepository().GetDocumentWithReferenceForModifyAsync(documentId);
            var medias = await _unitOfWork.GetMediaRepository().GetAllMediasWithTheSameDocnoForModifyAsync(documentId);
            Configuration foundedConfig = await _unitOfWork.GetConfigurationRepository()
                .Query().FirstOrDefaultAsync(a=>a.Type==(int)ConfigurationTypeEnum.FacePlusPlus);
            if (foundedConfig == null)
            {
                return ResultResponse<string>.Failure("Can't delete document");
            }
            bool? task = await _facePlusPlusClient.RemoveFaceAsync(foundedConfig.Value,document.Renter.FaceToken);
            if(task!=true)
            {
                return ResultResponse<string>.Failure("Can't delete document");

            }
            foreach (Media media in medias)
            {
                await _cloudinaryService.DeleteImageFileByUrlAsync(media.FileUrl,"Document");
            }
            document.Renter.FaceToken = null;
             _unitOfWork.GetDocumentRepository().Delete(document);
            await _unitOfWork.GetMediaRepository().DeleteRangeAsync(medias);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return ResultResponse<string>.SuccessResult("Delete document success",null);

        }
        catch (Exception ex)
        {

            await _unitOfWork.RollbackAsync();
            return ResultResponse<string>.Failure($"An error occurred: {ex.Message}");
        }
    }
  
    public async Task<ResultResponse<DocumentDetailResponse>> CreateUDocument(DocumentsCreateRequest documentCreateRequest)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            string renterId = _currentUserService.UserId;
            Renter renter = await _unitOfWork.GetRenterRepository().GetRenterByRenterIdAsync(Guid.Parse(renterId));
            var  fileList=  new List<IFormFile>
            {
                documentCreateRequest.BackDocumentFile,
                documentCreateRequest.FrontDocumentFile
            };
            if (renter == null)
                return ResultResponse<DocumentDetailResponse>.Failure("Renter not found");

            var document = new Document
            {
                DocumentNumber = documentCreateRequest.DocumentNumber,
                DocumentType = DocumentTypeEnum.Citizen.ToString(),
                IssueDate = documentCreateRequest.IssueDate,
                IssuingAuthority = documentCreateRequest.IssuingAuthority,
                VerificationStatus = documentCreateRequest.VerificationStatus,
                VerifiedAt = documentCreateRequest.VerifiedAt,
                ExpiryDate = documentCreateRequest.ExpiryDate,
                RenterId = renter.Id
            };
            var task= fileList.Select(async v=>
            {
                var url = await _cloudinaryService.UploadImageFileAsync(
                    v,
                     $"img_{Generator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                     "Document"
                    );
                return new Media
                {
                    DocNo = document.Id,
                    FileUrl = url,
                    EntityType = MediaEntityTypeEnum.Document.ToString(),
                    MediaType = MediaTypeEnum.Document.ToString(),
                };
            }
            ).ToList();
           
          

            string? faceToken = await _facePlusPlusClient.DetectFaceByUrlAsync(documentCreateRequest.FrontDocumentFile);
            if (string.IsNullOrEmpty(faceToken))
                return ResultResponse<DocumentDetailResponse>.Failure("Failed to detect face from image");

            Configuration foundedConfig = await _unitOfWork.GetConfigurationRepository()
               .Query().FirstOrDefaultAsync(a => a.Type == (int)ConfigurationTypeEnum.FacePlusPlus);
            bool added = await _facePlusPlusClient.AddFaceAsync(foundedConfig.Value, faceToken);
            if (!added)
                return ResultResponse<DocumentDetailResponse>.Failure("Failed to add face to FaceSet");

            renter.FaceToken = faceToken;
            List<Media> medis = (await Task.WhenAll(task)).ToList();

            await _unitOfWork.GetDocumentRepository().AddAsync(document);
            await _unitOfWork.GetMediaRepository().AddRangeAsync(medis);
            _unitOfWork.GetRenterRepository().Update(renter);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            List<string> docListUrls = medis.Select(i => i.FileUrl).ToList();
            var response = new DocumentDetailResponse
            {
                Id = document.Id,
                DocumentNumber = document.DocumentNumber,
                DocumentType = document.DocumentType,
                IssueDate = document.IssueDate,
                ExpiryDate = document.ExpiryDate,
                IssuingAuthority = document.IssuingAuthority,
                renter = new RenterResponse
                {
                    Id = renter.Id,
                    Address = renter.Address,
                    DateOfBirth = renter.DateOfBirth,
                    Email = renter.Email,
                    phone = renter.phone
                },
                fileUrl = docListUrls
            };

            return ResultResponse<DocumentDetailResponse>.SuccessResult("Document registered successfully", response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<DocumentDetailResponse>.Failure($"An error occurred: {ex.Message}");
        }
    }
    public async Task<ResultResponse<DocumentDetailResponse>> UpdateCitizenDocumentAsync(DocumentsUpdateRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var renterId = Guid.Parse(_currentUserService.UserId);
            var renter = await _unitOfWork.GetRenterRepository().GetRenterByRenterIdAsync(renterId);
            if (renter == null)
                return ResultResponse<DocumentDetailResponse>.Failure("Renter not found");

            var document = await _unitOfWork.GetDocumentRepository().FindByIdAsync(request.Id);
            if (document == null)
                return ResultResponse<DocumentDetailResponse>.Failure("Document not found");

            var existingMedias = await _unitOfWork.GetMediaRepository()
                .Query()
                .Where(m => m.DocNo == document.Id)
                .ToListAsync();
            document.DocumentNumber = request.DocumentNumber;
            document.IssueDate = request.IssueDate;
            document.ExpiryDate = request.ExpiryDate;
            document.IssuingAuthority = request.IssuingAuthority;
            document.VerificationStatus = request.VerificationStatus;
            document.VerifiedAt = request.VerifiedAt;
            var fileMap = new[]
            {
            new { MediaId = request.IdFileFront, File = request.FrontDocumentFile },
            new { MediaId = request.IdFileBack, File = request.BackDocumentFile }
        };

            var uploadTasks = fileMap.Select(async x =>
            {
                var media = existingMedias.FirstOrDefault(m => m.Id == x.MediaId);
                if (media == null)
                {
                    media = new Media
                    {
                        Id = Guid.NewGuid(),
                        DocNo = document.Id,
                        EntityType = MediaEntityTypeEnum.Document.ToString(),
                        MediaType = MediaTypeEnum.Document.ToString()
                    };
                    existingMedias.Add(media);
                }

                if (x.File == null)
                    return media;

                string? newUrl = await _cloudinaryService.UploadImageFileAsync(
                    x.File,
                    $"img_{Generator.PublicIdGenerate()}_{DateTime.Now:yyyyMMddHHmmss}",
                    "Document",
                    media.FileUrl
                );

                if (string.IsNullOrEmpty(newUrl))
                    throw new InvalidOperationException("Upload failed");

                media.FileUrl = newUrl;
                return media;
            }).ToList();

            var updatedMedias = (await Task.WhenAll(uploadTasks)).ToList();

            if (request.FrontDocumentFile != null)
            {
                Configuration faceConfig = await _unitOfWork.GetConfigurationRepository()
                    .Query().FirstOrDefaultAsync(a => a.Type == (int)ConfigurationTypeEnum.FacePlusPlus);

                if (!string.IsNullOrEmpty(renter.FaceToken))
                {
                    await _facePlusPlusClient.RemoveFaceAsync(faceConfig.Value, renter.FaceToken);
                }

                string? newToken = await _facePlusPlusClient.DetectFaceByUrlAsync(request.FrontDocumentFile);
                if (string.IsNullOrEmpty(newToken))
                    return ResultResponse<DocumentDetailResponse>.Failure("Failed to detect new face");

                bool added = await _facePlusPlusClient.AddFaceAsync(faceConfig.Value, newToken);
                if (!added)
                    return ResultResponse<DocumentDetailResponse>.Failure("Failed to add new face");

                renter.FaceToken = newToken;
                _unitOfWork.GetRenterRepository().Update(renter);
            }

   
            _unitOfWork.GetDocumentRepository().Update(document);
            foreach (var m in updatedMedias)
            {
                _unitOfWork.GetMediaRepository().Update(m);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            var response = new DocumentDetailResponse
            {
                Id = document.Id,
                DocumentNumber = document.DocumentNumber,
                DocumentType = document.DocumentType,
                IssueDate = document.IssueDate,
                ExpiryDate = document.ExpiryDate,
                IssuingAuthority = document.IssuingAuthority,
                renter = new RenterResponse
                {
                    Id = renter.Id,
                    Address = renter.Address,
                    DateOfBirth = renter.DateOfBirth,
                    Email = renter.Email,
                    phone = renter.phone
                },
                fileUrl = updatedMedias.Select(i => i.FileUrl).ToList()
            };

            return ResultResponse<DocumentDetailResponse>.SuccessResult("Citizen document updated successfully", response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<DocumentDetailResponse>.Failure($"An error occurred: {ex.Message}");
        }
    }



    public async Task<ResultResponse<DocumentDetailResponse>> CreateDrivingDocument(DocumentsCreateRequest documentCreateRequest)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            string renterId = _currentUserService.UserId;
            Renter renter = await _unitOfWork.GetRenterRepository().GetRenterByRenterIdAsync(Guid.Parse(renterId));
            var fileList = new List<IFormFile>
            {
                documentCreateRequest.BackDocumentFile,
                documentCreateRequest.FrontDocumentFile
            };
            if (renter == null)
                return ResultResponse<DocumentDetailResponse>.Failure("Renter not found");

            var document = new Document
            {
                DocumentNumber = documentCreateRequest.DocumentNumber,
                DocumentType = DocumentTypeEnum.Driving.ToString(),
                IssueDate = documentCreateRequest.IssueDate,
                IssuingAuthority = documentCreateRequest.IssuingAuthority,
                VerificationStatus = documentCreateRequest.VerificationStatus,
                VerifiedAt = documentCreateRequest.VerifiedAt,
                ExpiryDate = documentCreateRequest.ExpiryDate,
                RenterId = renter.Id
            };
            var task = fileList.Select(async v =>
            {
                var url = await _cloudinaryService.UploadImageFileAsync(
                    v,
                     $"img_{Generator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                     "Document"
                    );
                return new Media
                {
                    DocNo = document.Id,
                    FileUrl = url,
                    EntityType = MediaEntityTypeEnum.Document.ToString(),
                    MediaType = MediaTypeEnum.Document.ToString(),
                };
            }
            ).ToList();
            List<Media> medis = (await Task.WhenAll(task)).ToList();
            await _unitOfWork.GetDocumentRepository().AddAsync(document);
            await _unitOfWork.GetMediaRepository().AddRangeAsync(medis);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            List<string> docListUrls = medis.Select(i => i.FileUrl).ToList();
            var response = new DocumentDetailResponse
            {
                Id = document.Id,
                DocumentNumber = document.DocumentNumber,
                DocumentType = document.DocumentType,
                IssueDate = document.IssueDate,
                ExpiryDate = document.ExpiryDate,
                IssuingAuthority = document.IssuingAuthority,
                renter = new RenterResponse
                {
                    Id = renter.Id,
                    Address = renter.Address,
                    DateOfBirth = renter.DateOfBirth,
                    Email = renter.Email,
                    phone = renter.phone
                },
                fileUrl = docListUrls
            };

            return ResultResponse<DocumentDetailResponse>.SuccessResult("Document registered successfully", response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<DocumentDetailResponse>.Failure($"An error occurred: {ex.Message}");
        }
    }
    public async Task<ResultResponse<DocumentDetailResponse>> UpdateDrivingDocumentAsync(DocumentsUpdateRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var renterId = Guid.Parse(_currentUserService.UserId);
            var renter = await _unitOfWork.GetRenterRepository().GetRenterByRenterIdAsync(renterId);
            if (renter == null)
                return ResultResponse<DocumentDetailResponse>.Failure("Renter not found");
            var document = await _unitOfWork.GetDocumentRepository().FindByIdAsync(request.Id);
            if (document == null)
                return ResultResponse<DocumentDetailResponse>.Failure("Document not found");
            var existingMedias = await _unitOfWork.GetMediaRepository()
                .Query()
                .Where(m => m.DocNo == document.Id)
                .ToListAsync();
            document.DocumentNumber = request.DocumentNumber;
            document.IssueDate = request.IssueDate;
            document.ExpiryDate = request.ExpiryDate;
            document.IssuingAuthority = request.IssuingAuthority;
            document.VerificationStatus = request.VerificationStatus;
            document.VerifiedAt = request.VerifiedAt;
            var fileMap = new[]
            {
            new { MediaId = request.IdFileFront, File = request.FrontDocumentFile },
            new { MediaId = request.IdFileBack, File = request.BackDocumentFile }
        };
            var uploadTasks = fileMap.Select(async x =>
            {
                var media = existingMedias.FirstOrDefault(m => m.Id == x.MediaId);
                if (media == null)
                {
                    media = new Media
                    {
                        Id = Guid.NewGuid(),
                        DocNo = document.Id,
                        EntityType = MediaEntityTypeEnum.Document.ToString(),
                        MediaType = MediaTypeEnum.Document.ToString()
                    };
                    existingMedias.Add(media);
                }
                if (x.File == null)
                    return media;
                string? newUrl = await _cloudinaryService.UploadImageFileAsync(
                    x.File,
                    $"img_{Generator.PublicIdGenerate()}_{DateTime.Now:yyyyMMddHHmmss}",
                    "Document",
                    media.FileUrl
                );
                if (string.IsNullOrEmpty(newUrl))
                    throw new InvalidOperationException("Upload failed");

                media.FileUrl = newUrl;
                return media;
            }).ToList();

            var updatedMedias = (await Task.WhenAll(uploadTasks)).ToList();

            _unitOfWork.GetDocumentRepository().Update(document);
            foreach (var m in updatedMedias)
            {
                _unitOfWork.GetMediaRepository().Update(m);
            }
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            var response = new DocumentDetailResponse
            {
                Id = document.Id,
                DocumentNumber = document.DocumentNumber,
                DocumentType = document.DocumentType,
                IssueDate = document.IssueDate,
                ExpiryDate = document.ExpiryDate,
                IssuingAuthority = document.IssuingAuthority,
                renter = new RenterResponse
                {
                    Id = renter.Id,
                    Address = renter.Address,
                    DateOfBirth = renter.DateOfBirth,
                    Email = renter.Email,
                    phone = renter.phone
                },
                fileUrl = updatedMedias.Select(i => i.FileUrl).ToList()
            };
            return ResultResponse<DocumentDetailResponse>.SuccessResult("Citizen document updated successfully", response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<DocumentDetailResponse>.Failure($"An error occurred: {ex.Message}");
        }
    }


}
