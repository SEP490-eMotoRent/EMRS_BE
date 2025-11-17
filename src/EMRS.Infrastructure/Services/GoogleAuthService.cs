using Google.Apis.Auth;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.AccountDTOs;
using EMRS.Application.Interfaces.Repositories;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using EMRS.Application.DTOs.AuthDTOs;
using EMRS.Application.Abstractions.Models;

namespace EMRS.Infrastructure.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenProvider _tokenProvider;

        public GoogleAuthService(
            IUnitOfWork unitOfWork,
            ITokenProvider tokenProvider)
        {
            _unitOfWork = unitOfWork;
            _tokenProvider = tokenProvider;
        }

        public async Task<ResultResponse<LoginAccountResponse>> GoogleLoginAsync(GoogleLoginRequest request)
        {
            try
            {
                
                var payload = await VerifyGoogleTokenAsync(request.IdToken);

                if (payload == null)
                {
                    return ResultResponse<LoginAccountResponse>.Failure("Invalid Google token.");
                }

               
                var existingAccount = await _unitOfWork.GetAccountRepository()
                    .LoginAsync(payload.Email); 

                Account account;

                if (existingAccount != null)
                {
                   
                    account = existingAccount;
                }
                else
                {
                    
                    var createResult = await CreateGoogleAccountAsync(payload);

                    if (!createResult.Success)
                    {
                        return ResultResponse<LoginAccountResponse>.Failure(createResult.Message);
                    }

                    account = createResult.Data;
                }

                
                string avatarUrl = null;
                var renterMedia = await _unitOfWork.GetMediaRepository()
                    .GetMediasByEntityIdAsync(account.Renter.Id);

                if (renterMedia != null && renterMedia.Any())
                {
                    avatarUrl = renterMedia.FirstOrDefault()?.FileUrl;
                }


                if (string.IsNullOrEmpty(avatarUrl) && !string.IsNullOrEmpty(payload.Picture))
                {
                    avatarUrl = payload.Picture; 
                }

                
                var token = _tokenProvider.JWTGenerator(account);

                
                var response = new LoginAccountResponse
                {
                    AccessToken = token,
                    User = new User
                    {
                        AvatarUrl = avatarUrl,
                        Username = account.Username,
                        Id = account.Renter.Id,
                        FullName = account.Fullname,
                        Role = account.Role
                    }
                };

                return ResultResponse<LoginAccountResponse>.SuccessResult(
                    "Google login successful.",
                    response);
            }
            catch (Exception ex)
            {
                return ResultResponse<LoginAccountResponse>.Failure(
                    $"An error occurred during Google login: {ex.Message}");
            }
        }


        private async Task<GoogleJsonWebSignature.Payload> VerifyGoogleTokenAsync(string idToken)
        {
            try
            {
                var googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");

                if (string.IsNullOrEmpty(googleClientId))
                {
                    throw new InvalidOperationException("GOOGLE_CLIENT_ID environment variable is not set.");
                }

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { googleClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return payload;
            }
            catch (InvalidJwtException)
            {
                return null;
            }
        }


        private async Task<ResultResponse<Account>> CreateGoogleAccountAsync(
            GoogleJsonWebSignature.Payload payload)
        {
            try
            {
               
                var defaultMembership = await _unitOfWork.GetMembershipRepository()
                    .FindLowestMinBookingMembershipAsync();

                if (defaultMembership == null)
                {
                    return ResultResponse<Account>.Failure("Default membership not found.");
                }

                
                var newAccount = new Account
                {
                    Username = payload.Email, 
                    Fullname = payload.Name ?? $"{payload.GivenName} {payload.FamilyName}".Trim(),
                    Password = GenerateRandomPassword(),
                    Role = UserRoleName.RENTER.ToString(),

                    Renter = new Renter
                    {
                        Email = payload.Email,
                        phone = "", 
                        Address = "", 
                        DateOfBirth = null, 
                        IsVerified = payload.EmailVerified, 
                        MembershipId = defaultMembership.Id,
                        VerificationCode = string.Empty, 
                        VerificationCodeExpiry = null,

                        Wallet = new Wallet
                        {
                            Balance = 0 
                        }
                    }
                };

                
                await _unitOfWork.GetAccountRepository().AddAsync(newAccount);
                await _unitOfWork.SaveChangesAsync();

                return ResultResponse<Account>.SuccessResult(
                    "Account created successfully.",
                    newAccount);
            }
            catch (Exception ex)
            {
                return ResultResponse<Account>.Failure(
                    $"Failed to create account: {ex.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string GenerateRandomPassword()
        {
            return Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
        }
    }
}