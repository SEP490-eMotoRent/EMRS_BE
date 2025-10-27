using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.AccountDTOs;
using EMRS.Application.DTOs.MembershipDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICloudinaryService _cloudinaryService;
    public AccountService(IUnitOfWork unitOfWork,ICurrentUserService currentUserService,ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _cloudinaryService = cloudinaryService;
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
    public async Task<ResultResponse<RenterAccountUpdateResponse>> UpdateUserProfile(RenterAccountUpdateRequest renterAccountUpdateRequest)
    {
        try
        {
            var renterId = Guid.Parse(_currentUserService.UserId);
            var renter = await _unitOfWork.GetRenterRepository().GetRenterByAccountIdAsync(renterId);
            var account = await _unitOfWork.GetAccountRepository().FindByIdAsync(renter.AccountId);
            var check = await _unitOfWork.GetAccountRepository().GetByEmaiAndUsernameAsync(renterAccountUpdateRequest.Email,account.Username);
            if(check == true && renterAccountUpdateRequest.Email != account.Renter.Email)
            {
                return ResultResponse<RenterAccountUpdateResponse>.Failure("Email is already in use.");
            }
            string responseUrl = null;

           
            if (renterAccountUpdateRequest.ProfilePicture != null)
            {
              
                if (renterAccountUpdateRequest.MediaId != null)
                {
                    var media = await _unitOfWork.GetMediaRepository().FindByIdAsync(renterAccountUpdateRequest.MediaId.Value);
                    var urlString = await _cloudinaryService.UploadImageFileAsync(
                        renterAccountUpdateRequest.ProfilePicture,
                        $"img_{PublicIdGenerator.PublicIdGenerate()}_{DateTime.Now:yyyyMMddHHmmss}",
                        "Images",
                        media.FileUrl
                    );
                    media.FileUrl = urlString;
                    _unitOfWork.GetMediaRepository().Update(media);
                    responseUrl = urlString;
                }
                else
                {
                    var urlString = await _cloudinaryService.UploadImageFileAsync(
                        renterAccountUpdateRequest.ProfilePicture,
                        $"img_{PublicIdGenerator.PublicIdGenerate()}_{DateTime.Now:yyyyMMddHHmmss}",
                        "Images"
                    );

                    var media = new Media
                    {
                        FileUrl = urlString,
                        DocNo = renterId,
                        EntityType = MediaEntityTypeEnum.Renter.ToString(),
                        MediaType = MediaTypeEnum.Image.ToString(),
                    };

                    await _unitOfWork.GetMediaRepository().AddAsync(media);
                    responseUrl = urlString;
                }
            }
            else
            {
                if (renterAccountUpdateRequest.MediaId != null)
                {
                    var existingMedia = await _unitOfWork.GetMediaRepository().FindByIdAsync(renterAccountUpdateRequest.MediaId.Value);
                    responseUrl = existingMedia?.FileUrl;
                }
                else
                {
                    responseUrl = null; 
                }
            }
            account.Fullname = renterAccountUpdateRequest.Fullname;
            account.Renter.Email = renterAccountUpdateRequest.Email;
            account.Renter.Address = renterAccountUpdateRequest.Address;
            account.Renter.phone = renterAccountUpdateRequest.phone;
            account.Renter.DateOfBirth = renterAccountUpdateRequest.DateOfBirth;

            _unitOfWork.GetAccountRepository().Update(account);
            await _unitOfWork.SaveChangesAsync();

            var response = new RenterAccountUpdateResponse
            {
                Fullname = account.Fullname,
                Email = account.Renter.Email,
                Address = account.Renter.Address,
                phone = account.Renter.phone,
                DateOfBirth = account.Renter.DateOfBirth,
                ProfilePicture = responseUrl 
            };

            return ResultResponse<RenterAccountUpdateResponse>.SuccessResult("Profile updated successfully", response);
        }
        catch (Exception ex)
        {
            return ResultResponse<RenterAccountUpdateResponse>.Failure($"An error occurred: {ex.Message}");
        }
    }


    /*  public async Task<ResultResponse<>> CreateAccountAsync(CreateAccountRequest createAccountRequest)
      {
          var newAccount = new Account
          {
              Username = registerUserRequest.Username,
              Fullname = registerUserRequest.Fullname,
              Password = passwordHash,
              Role = UserRoleName.STAFF.ToString(),

              Staff = new Staff
              {
                  BranchId = createAccountRequest.BranchId,

              }


          };
      }*/
}
