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
            var verificationCode = GenerateVerificationCode();
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
                    AvatarUrl = registerUserRequest.AvatarUrl,
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
            registerRenterResponse = _mapper.Map<RegisterRenterResponse>(newAccount.Renter);
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

 
    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString(); 
    }

    public async Task<ResultResponse<string>> LoginAsync(LoginAccountRequest loginAccountRequest)
    {
        try
        {
            var account = await _unitOfWork.GetAccountRepository().LoginAsync(loginAccountRequest.Username);

            var checkPassword = _passwordHasher.Verify(loginAccountRequest.Password, account.Password);

            if (account == null || !checkPassword)
            {
                return ResultResponse<string>.Failure("Invalid username or password.");
            }
            var token = _tokenProvider.JWTGenerator(account);
            return ResultResponse<string>.SuccessResult("Login successful.", token);
        }
        catch (Exception ex)
        {
            return ResultResponse<string>.Failure($"An error occurred during login: {ex.Message}");

        }
    }
}
