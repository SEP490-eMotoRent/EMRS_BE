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

namespace EMRS.Application.Services
{
    public interface IGoogleAuthService
    {
        Task<ResultResponse<LoginAccountResponse>> GoogleLoginAsync(GoogleLoginRequest request);
    }

    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenProvider _tokenProvider;

        public GoogleAuthService(
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            ITokenProvider tokenProvider)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _tokenProvider = tokenProvider;
        }

        public async Task<ResultResponse<LoginAccountResponse>> GoogleLoginAsync(GoogleLoginRequest request)
        {
            try
            {
                // 1. Verify Google ID Token
                var payload = await VerifyGoogleTokenAsync(request.IdToken);

                if (payload == null)
                {
                    return ResultResponse<LoginAccountResponse>.Failure("Invalid Google token.");
                }

                // 2. Kiểm tra email đã tồn tại chưa
                var existingAccount = await _unitOfWork.GetAccountRepository()
                    .LoginAsync(payload.Email); // Dùng email làm username

                Account account;

                if (existingAccount != null)
                {
                    // User đã tồn tại - Login
                    account = existingAccount;
                }
                else
                {
                    // User mới - Tạo account
                    var createResult = await CreateGoogleAccountAsync(payload);

                    if (!createResult.Success)
                    {
                        return ResultResponse<LoginAccountResponse>.Failure(createResult.Message);
                    }

                    account = createResult.Data;
                }

                // 3. Lấy avatar nếu có
                string avatarUrl = null;
                var renterMedia = await _unitOfWork.GetMediaRepository()
                    .GetMediasByEntityIdAsync(account.Renter.Id);

                if (renterMedia != null && renterMedia.Any())
                {
                    avatarUrl = renterMedia.FirstOrDefault()?.FileUrl;
                }

                // Nếu chưa có avatar, có thể lưu avatar từ Google
                if (string.IsNullOrEmpty(avatarUrl) && !string.IsNullOrEmpty(payload.Picture))
                {
                    avatarUrl = payload.Picture; // URL avatar từ Google
                }

                // 4. Tạo JWT token
                var token = _tokenProvider.JWTGenerator(account);

                // 5. Tạo response
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

        /// <summary>
        /// Verify Google ID Token với Google servers
        /// </summary>
        private async Task<GoogleJsonWebSignature.Payload> VerifyGoogleTokenAsync(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Authentication:Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return payload;
            }
            catch (InvalidJwtException)
            {
                return null;
            }
        }

        /// <summary>
        /// Tạo Account và Renter mới từ Google profile
        /// </summary>
        private async Task<ResultResponse<Account>> CreateGoogleAccountAsync(
            GoogleJsonWebSignature.Payload payload)
        {
            try
            {
                // Lấy membership thấp nhất (MinBooking = 0)
                var defaultMembership = await _unitOfWork.GetMembershipRepository()
                    .FindLowestMinBookingMembershipAsync();

                if (defaultMembership == null)
                {
                    return ResultResponse<Account>.Failure("Default membership not found.");
                }

                // Tạo Account mới
                var newAccount = new Account
                {
                    Username = payload.Email, // Dùng email làm username
                    Fullname = payload.Name ?? $"{payload.GivenName} {payload.FamilyName}".Trim(),
                    Password = GenerateRandomPassword(), // Password ngẫu nhiên (không dùng)
                    Role = UserRoleName.RENTER.ToString(),

                    Renter = new Renter
                    {
                        Email = payload.Email,
                        phone = "", // Để trống, user sẽ cập nhật sau
                        Address = "", // Để trống, user sẽ cập nhật sau
                        DateOfBirth = null, // Để null, user sẽ cập nhật sau
                        IsVerified = payload.EmailVerified, // Google đã verify email
                        MembershipId = defaultMembership.Id,
                        VerificationCode = string.Empty, // Không cần verification code
                        VerificationCodeExpiry = null,

                        Wallet = new Wallet
                        {
                            Balance = 0 
                        }
                    }
                };

                // Lưu vào database
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
        /// Tạo password ngẫu nhiên (không sử dụng thực tế vì login qua Google)
        /// </summary>
        private string GenerateRandomPassword()
        {
            return Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
        }
    }
}