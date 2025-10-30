using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.DocumentDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class DocumentService:IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    public DocumentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
   /* public async Task<ResultResponse<>> CreateUserCollectionAndDocument(DocumentCreateRequest documentCreateRequest)
    {
        try
        {
            Renter foundedAccount = await _unitOfWork.GetRenterRepository().FindByIdAsync(documentCreateRequest.RenterId);

            await  IFptFaceSearchClient.CreateUserAsync(foundedAccount.Account.Fullname);
        }
        catch (Exception ex)
        {

        }

    }*/
}
