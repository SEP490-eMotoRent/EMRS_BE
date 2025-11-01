using EMRS.Application.Common;
using EMRS.Application.DTOs.AccountDTOs;
using EMRS.Application.DTOs.MembershipDTOs;
using EMRS.Application.DTOs.RenterDTOs;
using EMRS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services;

public interface IAccountService
{
    Task<ResultResponse<List<AccountDetailResponse>>> GetAllAccountAsync();
    Task<ResultResponse<RenterDetailResponse>> GetRenterDetail(Guid renterId);
    Task<ResultResponse<RenterScannerResponse>> ScanAndReturnRenterInfo(IFormFile image);
    Task<ResultResponse<Membership>> CreateMembership(CreateMembershipRequest createMembershipRequest);
    Task<ResultResponse<RenterAccountUpdateResponse>> UpdateUserProfile(RenterAccountUpdateRequest renterAccountUpdateRequest);
    Task<ResultResponse<string>> DeleteScanerFace(string url);
    Task<ResultResponse<CreateStaffAccountResponse>> CreateManagerAccount(CreateManagerRequest request);
}
