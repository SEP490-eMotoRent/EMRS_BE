using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.MembershipDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    public AccountService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ResultResponse<Membership>> CreateMembership(CreateMembershipRequest createMembershipRequest)
    {
      
            var newMembership = new Membership
            {
                TierName = createMembershipRequest.TierName,
                Description = createMembershipRequest.Description,
                FreeChargingPerMonth = createMembershipRequest.FreeChargingPerMonth,
                DiscountPercentage = createMembershipRequest.DiscountPercentage,
                MinBookings = createMembershipRequest.MinBookings
            };
            await _unitOfWork.GetMembershipRepository().AddAsync(newMembership);
            await _unitOfWork.SaveChangesAsync();
            return ResultResponse<Membership>.SuccessResult("Membership created", newMembership);
       
        
    }
}
