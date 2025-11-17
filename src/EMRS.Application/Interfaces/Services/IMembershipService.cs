using EMRS.Application.Common;
using EMRS.Application.DTOs.MembershipDTOs;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services
{
    public  interface IMembershipService
    {
        Task<ResultResponse<Membership>> CreateMembership(CreateMembershipRequest createMembershipRequest);
        Task<ResultResponse<List<MembershipResponse>>> GetAllMembershipsAsync();
        Task<ResultResponse<MembershipResponse>> GetMembershipByIdAsync(Guid id);
        Task<ResultResponse<MembershipResponse>> UpdateAsync(UpdateMembershipRequest request);
        Task<ResultResponse<bool>> DeleteAsync(Guid id);
        

        }
}
