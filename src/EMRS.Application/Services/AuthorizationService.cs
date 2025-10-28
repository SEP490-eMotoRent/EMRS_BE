using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.AccountDTOs;
using EMRS.Application.DTOs.RenterDTOs;
using EMRS.Application.Interfaces.Repositories;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public  class AuthorizationService:IAuthorizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly ITokenProvider _tokenProvider;
    public AuthorizationService(IMapper mapper,
        ITokenProvider tokenProvider,
        IEmailService emailService,IUnitOfWork unitOfWork,IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _tokenProvider = tokenProvider;
    }

    public async Task<ResultResponse<RegisterRenterResponse>> RegisterUserAsync(RegisterUserRequest registerUserRequest)
    {
        try
        {
            var check= await _unitOfWork.GetAccountRepository().GetByEmaiAndUsernameAsync(registerUserRequest.Email, registerUserRequest.Username);
            if (check == true )
            {
                return ResultResponse<RegisterRenterResponse>.Failure("Email/Username is already in use.");
            }
            var verificationCode = Generator.GenerateVerificationCode();
            int minutesToExpire = 10;
            var verificationExpiry = DateTime.UtcNow.AddMinutes(minutesToExpire);
            RegisterRenterResponse registerRenterResponse = new RegisterRenterResponse();
            var passwordHash = _passwordHasher.Hash(registerUserRequest.Password);
            if (registerUserRequest == null)
                return ResultResponse<RegisterRenterResponse>.Failure("Invalid user data.");
            
            var existingAccount = await _unitOfWork
                .GetAccountRepository()
                .GetByEmaiAndUsernameAsync(registerUserRequest.Email,registerUserRequest.Username);
            var existingMembership = await _unitOfWork
              .GetMembershipRepository()
             .FindLowestMinBookingMembershipAsync();
            if (existingMembership == null)
                return ResultResponse<RegisterRenterResponse>.Failure("Default membership not found.");
            if (existingAccount)
                return ResultResponse<RegisterRenterResponse>.Failure("User/Email already in use.");

            var newAccount = new Account
            {
                Username = registerUserRequest.Username,
                Fullname = registerUserRequest.Fullname,
                Password = passwordHash,
                Role = UserRoleName.RENTER.ToString(),
                
                Renter = new Renter
                {
                    Address = registerUserRequest.Address,
                    DateOfBirth = registerUserRequest.DateOfBirth,
                  
                    Email = registerUserRequest.Email,
                    phone = registerUserRequest.phone,
                    VerificationCode = verificationCode,
                    VerificationCodeExpiry = verificationExpiry,
                    MembershipId = existingMembership.Id,
                    Wallet = new Wallet
                    {
                        
                        Balance = 1000000000000,
                    }
                },
                

            };
            registerRenterResponse = new RegisterRenterResponse
            {
                Id = newAccount.Renter.Id,
                Email = newAccount.Renter.Email,
                phone = newAccount.Renter.phone,
                Address = newAccount.Renter.Address,
                DateOfBirth = newAccount.Renter.DateOfBirth,
                MembershipId = newAccount.Renter.MembershipId,
                Username = newAccount.Username,
                Fullname = newAccount.Fullname,
                VerificationCodeExpiry = newAccount.Renter.VerificationCodeExpiry
            };
            await _unitOfWork.GetAccountRepository().AddAsync(newAccount);
            
            await _unitOfWork.SaveChangesAsync();

            await _emailService.SendVerificationEmailAsync(registerUserRequest.Email, verificationCode, minutesToExpire);
           


            return ResultResponse<RegisterRenterResponse>.SuccessResult( "User registered successfully. Verification email sent.", registerRenterResponse);

        }
        catch (Exception ex)
        {
            return ResultResponse<RegisterRenterResponse>.Failure($"An error occurred while registering the user: {ex.Message}");
        }
    }

 
  
    public async Task<ResultResponse<LoginAccountResponse>> LoginAsync(LoginAccountRequest loginAccountRequest)
    {
        try
        {
            var account = await _unitOfWork.GetAccountRepository().LoginAsync(loginAccountRequest.Username);

            var checkPassword = _passwordHasher.Verify(loginAccountRequest.Password, account.Password);

            string avatarUrl = null;
            if (account.Role == UserRoleName.RENTER.ToString())
            {
                var renterMedia = await _unitOfWork.GetMediaRepository().GetMediasByEntityIdAsync(account.Renter.Id);
                if (renterMedia != null)
                {
                    avatarUrl = renterMedia.FirstOrDefault()?.FileUrl;
                }

            }
            else
            {
                avatarUrl = null;
            }

            if (account == null || !checkPassword)
            {
                return ResultResponse<LoginAccountResponse>.Failure("Invalid username or password.");
            }
            var token = _tokenProvider.JWTGenerator(account);
            LoginAccountResponse response;
            if (account.Role == UserRoleName.RENTER.ToString()) { 
            response = new LoginAccountResponse
            {
                AccessToken = token,
                User = new User
                {
                    AvatarUrl = avatarUrl,
                    Username = account.Username,
                    Id = account.Id,
                    FullName = account.Fullname,
                    Role = account.Role

                }
            };
            }
            else
            {
                response = new LoginAccountResponse
                {
                    AccessToken = token,
                    User = new User
                    {
                        AvatarUrl = null,
                        Username = account.Username,
                        Id = account.Id,
                        FullName = account.Fullname,
                        Role = account.Role,
                        BranchId= account.Staff != null ? account.Staff.BranchId : null,
                        BranchName =  account.Staff.Branch != null ? account.Staff.Branch.BranchName : null
                    }
                };
            }
                return ResultResponse<LoginAccountResponse>.SuccessResult("Login successful.", response);
        }
        catch (Exception ex)
        {
            return ResultResponse<LoginAccountResponse>.Failure($"An error occurred during login: {ex.Message}");

        }
    }
}
