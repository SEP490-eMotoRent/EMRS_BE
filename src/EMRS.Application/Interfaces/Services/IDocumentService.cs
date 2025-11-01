using EMRS.Application.Common;
using EMRS.Application.DTOs.DocumentDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services;

public interface IDocumentService
{
    Task<ResultResponse<string>> DeleteDocumentAsync(Guid documentId);
    Task<ResultResponse<DocumentDetailResponse>> CreateDrivingDocument(DocumentsCreateRequest documentCreateRequest);
    Task<ResultResponse<DocumentDetailResponse>> UpdateDrivingDocumentAsync(DocumentsUpdateRequest request);
    Task<ResultResponse<DocumentDetailResponse>> CreateUDocument(DocumentsCreateRequest documentCreateRequest);
    Task<ResultResponse<DocumentDetailResponse>> UpdateCitizenDocumentAsync(DocumentsUpdateRequest request);
}
