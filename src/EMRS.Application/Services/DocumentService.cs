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
  
    public async Task<ResultResponse<DocumentDetalResponse>> CreateUDocument(DocumentCreateRequest documentCreateRequest)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            string renterId = _currentUserService.UserId;
            Renter renter = await _unitOfWork.GetRenterRepository().GetRenterByRenterIdAsync(Guid.Parse(renterId));

            if (renter == null)
                return ResultResponse<DocumentDetalResponse>.Failure("Renter not found");

        
            string fileUrl = await _cloudinaryService.UploadImageFileAsync(
                documentCreateRequest.DocumentFile,
                $"img_{Generator.PublicIdGenerate()}_{DateTime.Now:yyyyMMddHHmmss}",
                "Document"
            );

         
            string? faceToken = await _facePlusPlusClient.DetectFaceByUrlAsync(fileUrl);
            if (string.IsNullOrEmpty(faceToken))
                return ResultResponse<DocumentDetalResponse>.Failure("Failed to detect face from image");

            Configuration foundedConfig = await _unitOfWork.GetConfigurationRepository()
               .Query().FirstOrDefaultAsync(a => a.Type == (int)ConfigurationTypeEnum.FacePlusPlus);
        

            bool added = await _facePlusPlusClient.AddFaceAsync(foundedConfig.Value, faceToken);
            if (!added)
                return ResultResponse<DocumentDetalResponse>.Failure("Failed to add face to FaceSet");

            renter.FaceToken = faceToken;

           
            var document = new Document
            {
                DocumentNumber = documentCreateRequest.DocumentNumber,
                DocumentType = documentCreateRequest.DocumentType.ToString(),
                IssueDate = documentCreateRequest.IssueDate,
                IssuingAuthority = documentCreateRequest.IssuingAuthority,
                VerificationStatus = documentCreateRequest.VerificationStatus,
                VerifiedAt = documentCreateRequest.VerifiedAt,
                ExpiryDate = documentCreateRequest.ExpiryDate,
                RenterId = renter.Id
            };

            var media = new Media
            {
                DocNo = document.Id,
                EntityType = MediaEntityTypeEnum.Document.ToString(),
                MediaType = MediaTypeEnum.Image.ToString(),
                FileUrl = fileUrl
            };

            await _unitOfWork.GetDocumentRepository().AddAsync(document);
            await _unitOfWork.GetMediaRepository().AddAsync(media);
            _unitOfWork.GetRenterRepository().Update(renter);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            var response = new DocumentDetalResponse
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
                fileUrl = fileUrl
            };

            return ResultResponse<DocumentDetalResponse>.SuccessResult("Document registered successfully", response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<DocumentDetalResponse>.Failure($"An error occurred: {ex.Message}");
        }
    }
}
