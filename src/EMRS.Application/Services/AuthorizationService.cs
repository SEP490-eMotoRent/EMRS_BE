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
    public AuthorizationService(IMapper mapper,
        IEmailService emailService,IUnitOfWork unitOfWork,IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
        _mapper = mapper;   
    }
    public async Task<ResultResponse<RegisterRenterResponse>> RegisterUser(RegisterUserRequest registerUserRequest)
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
                .GetByEmaiAsync(registerUserRequest.Email);
            var existingMembership = await _unitOfWork
              .GetMembershipRepository()
              .FindByIdAsync(Guid.Parse("0199f191-10e4-763c-9859-c01f1025b530"));
            if (existingMembership == null)
                return ResultResponse<RegisterRenterResponse>.Failure("Default membership not found.");
            if (existingAccount != null)
                return ResultResponse<RegisterRenterResponse>.Failure("Email already in use.");

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
                    Membership = existingMembership,
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

}
