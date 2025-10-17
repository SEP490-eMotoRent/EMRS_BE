using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.AccountDTOs;
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
    public AuthorizationService(IEmailService emailService,IUnitOfWork unitOfWork,IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
    }
    public async Task<ResultResponse<Account>> RegisterUser(RegisterUserRequest registerUserRequest)
    {
        try
        {
            var verificationCode = GenerateVerificationCode();
            int minutesToExpire = 10;
            var verificationExpiry = DateTime.UtcNow.AddMinutes(minutesToExpire);

            var passwordHash = _passwordHasher.Hash(registerUserRequest.Password);
            if (registerUserRequest == null)
                return ResultResponse<Account>.Failure("Invalid user data.");

            var existingAccount = await _unitOfWork
                .GetAccountRepository()
                .GetByEmaiAsync(registerUserRequest.Email);

            if (existingAccount != null)
                return ResultResponse<Account>.Failure("Email already in use.");

            var newAccount = new Account
            {
                Username = registerUserRequest.Username,
                Fullname = registerUserRequest.Fullname,
                Password = passwordHash,
                Role = UserRoleName.RENTER.ToString(),
                
            };
            var newRenter = new Renter
            {
                Address = registerUserRequest.Address,
                DateOfBirth = registerUserRequest.DateOfBirth,
                AvatarUrl = registerUserRequest.AvatarUrl,
                Email = registerUserRequest.Email,
                phone = registerUserRequest.phone,
                VerificationCode = verificationCode,
                VerificationCodeExpiry = verificationExpiry,
            };
            await _unitOfWork.GetAccountRepository().AddAsync(newAccount);
            await _unitOfWork.SaveChangesAsync();

            await _emailService.SendVerificationEmailAsync(registerUserRequest.Email, verificationCode, minutesToExpire);

            return ResultResponse<Account>.SuccessResult( "User registered successfully. Verification email sent.", newAccount);

        }
        catch (Exception ex)
        {
            return ResultResponse<Account>.Failure($"An error occurred while registering the user: {ex.Message}");
        }
    }

 
    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString(); 
    }

}
