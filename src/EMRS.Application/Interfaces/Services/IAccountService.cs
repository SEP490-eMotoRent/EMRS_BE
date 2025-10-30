using EMRS.Application.Common;
using EMRS.Application.DTOs.AccountDTOs;
using EMRS.Application.DTOs.MembershipDTOs;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services;

public interface IAccountService
{
    Task<ResultResponse<List<AccountDetailResponse>>> GetAllAccountAsync();
    Task<ResultResponse<Membership>> CreateMembership(CreateMembershipRequest createMembershipRequest);
    Task<ResultResponse<RenterAccountUpdateResponse>> UpdateUserProfile(RenterAccountUpdateRequest renterAccountUpdateRequest);
}
