using EMRS.Application.Abstractions;
using EMRS.Application.Abstractions.Models.FacePlusPlus;
using EMRS.Application.Common;
using EMRS.Application.DTOs.AccountDTOs;
using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.DocumentDTOs;
using EMRS.Application.DTOs.MembershipDTOs;
using EMRS.Application.DTOs.RenterDTOs;
using EMRS.Application.DTOs.StaffDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IFacePlusPlusService _facePlusPlusClient;
    private readonly IPasswordHasher _passwordHasher;
    public AccountService(IPasswordHasher passwordHasher,IFacePlusPlusService facePlusPlusClient,IUnitOfWork unitOfWork,ICurrentUserService currentUserService,ICloudinaryService cloudinaryService)
    {
        _facePlusPlusClient= facePlusPlusClient;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _cloudinaryService = cloudinaryService;
        _passwordHasher = passwordHasher;
    }

   
    public async Task<ResultResponse<List<AccountDetailResponse>>> GetAllAccountAsync()
    {
        try {
            var accounts = await _unitOfWork.GetAccountRepository().GetAccountsWithReferenceAsync();
            var roleCheck = UserRoleName.RENTER.ToString();
            var response = accounts.Select(a =>
            {
               
                return new AccountDetailResponse
                {
                    Id = a.Id,
                    Fullname = a.Fullname,
                    Username = a.Username,
                    Role = a.Role,

                    staff =  a.Role!=roleCheck && a.Staff != null
                        ? new StaffResponse
                        {
                            Id = a.Staff.Id,
                            Branch = a.Staff.Branch != null
                                ? new BranchResponse
                                {
                                    Id = a.Staff.Branch.Id,
                                    Phone = a.Staff.Branch.Phone,
                                    Address = a.Staff.Branch.Address,
                                    BranchName = a.Staff.Branch.BranchName,
                                    City = a.Staff.Branch.City,
                                    ClosingTime = a.Staff.Branch.ClosingTime,
                                    Email = a.Staff.Branch.Email,
                                    Latitude = a.Staff.Branch.Latitude,
                                    Longitude = a.Staff.Branch.Longitude,
                                    OpeningTime = a.Staff.Branch.OpeningTime
                                }
                                : null
                        }
                        : null,

                    renter = a.Role == roleCheck && a.Renter != null
                        ? new RenterResponse
                        {
                            Id = a.Renter.Id,
                            Email = a.Renter.Email,
                            Address = a.Renter.Address,
                            DateOfBirth = a.Renter.DateOfBirth,
                            phone = a.Renter.phone
                        }
                        : null
                };
            }).ToList();

            return ResultResponse<List<AccountDetailResponse>>.SuccessResult("", response);
        }
        catch (Exception ex) {
            return ResultResponse<List<AccountDetailResponse>>.Failure($"An error occurred: {ex.Message}");
        }
    }
   
    public async Task<ResultResponse<RenterAccountUpdateResponse>> UpdateUserProfile(RenterAccountUpdateRequest renterAccountUpdateRequest)
    {
        try
        {
            var renterId = Guid.Parse(_currentUserService.UserId);
            var renter = await _unitOfWork.GetRenterRepository().GetRenterByRenterIdAsync(renterId);
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
                        $"img_{Generator.PublicIdGenerate()}_{DateTime.Now:yyyyMMddHHmmss}",
                        "Renter",
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
                        $"img_{Generator.PublicIdGenerate()}_{DateTime.Now:yyyyMMddHHmmss}",
                        "Renter"
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
    public async Task<ResultResponse<string>> DeleteScanerFace(string url)
    {
        try
        {

            var task=await _cloudinaryService.DeleteImageFileByUrlAsync(url, "FaceScan");
            if (task==false)
                return ResultResponse<string>.Failure("Delete Failed");

            return ResultResponse<string>.SuccessResult("Delete Success", null);
        }
        catch (Exception ex)
        {
            return ResultResponse<string>.Failure($"An error occurred: {ex.Message}");
        }
    }
    public async Task<ResultResponse<RenterScannerResponse>> ScanAndReturnRenterInfo(IFormFile image)
    {
        try
        {
            Configuration foundedConfig = await _unitOfWork.GetConfigurationRepository()
              .Query().FirstOrDefaultAsync(a => a.Type == (int)ConfigurationTypeEnum.FacePlusPlus);
            FaceSearchResult faceSearchResult = await _facePlusPlusClient.SearchByFileAsync(image, foundedConfig.Value);
            if (faceSearchResult == null)
            {
                return ResultResponse<RenterScannerResponse>.Failure("An error occurred while searching for renter face");
            }

            Renter renter = await _unitOfWork.GetRenterRepository()
                .Query().Include(a=>a.Account).FirstOrDefaultAsync(a => a.FaceToken == faceSearchResult.Id);
            if (renter == null)
            {
                return ResultResponse<RenterScannerResponse>.Failure("Can't find any renter ");
            }
            Media avatar = await _unitOfWork.GetMediaRepository()
              .GetAMediaWithCondAsync(renter.Id, MediaEntityTypeEnum.Renter.ToString());
            var url = await _cloudinaryService.UploadImageFileAsync(
                image,
                  $"img_{Generator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                  "FaceScan"
                );
            var media = new Media
            {
                FileUrl = url,
                DocNo = renter.Id,
                EntityType = MediaEntityTypeEnum.RenterFaceScan.ToString(),
                MediaType = MediaTypeEnum.Image.ToString(),
            };
            if (url == null)
            {
                return ResultResponse<RenterScannerResponse>.Failure("Problem saving scanner face ");
            }
            var response = new RenterScannerResponse
            {
                Id = renter.Id,
                Address = renter.Address,
                DateOfBirth = renter.DateOfBirth,
                Email = renter.Email,
                phone = renter.phone,
                AvatarUrl = avatar?.FileUrl,
                account = new AccountResponse
                {
                    Id = renter.Account.Id,
                    Fullname = renter.Account.Fullname,
                    Role = renter.Account.Role,
                    Username = renter.Account.Username,
                    
                },
                FaceScanUrl=url
            };
            await _unitOfWork.GetMediaRepository().AddAsync(media);

            return ResultResponse<RenterScannerResponse>.SuccessResult("Renter found", response);

        }
        catch (Exception ex)
        {
            return ResultResponse<RenterScannerResponse>.Failure($"An error occurred: {ex.Message}");
        }
    }
    public async Task<ResultResponse<RenterDetailResponse>> GetRenterDetail(Guid renterId)
    {

        try
        {
            Renter renter= await _unitOfWork.GetRenterRepository().GetRenterByRenterIdAsync(renterId);
            Media avatar = await _unitOfWork.GetMediaRepository()
                .GetAMediaWithCondAsync(renterId, MediaEntityTypeEnum.Renter.ToString());
            var listDoc = await _unitOfWork.GetDocumentRepository().GetDocumentByRenterIdAsync(renterId);
            var media = await _unitOfWork.GetMediaRepository().Query().Where(a => a.EntityType == MediaEntityTypeEnum.Document.ToString()).ToListAsync();
            var mediaDict= media.GroupBy(a=>a.DocNo)
                .ToDictionary(a=>a.Key, a=>a.ToList());
            var response = new RenterDetailResponse
            {
                Id = renterId,
                Address = renter.Address,
                DateOfBirth = renter.DateOfBirth,
                Email = renter.Email,
                phone = renter.phone,
                AvatarUrl = avatar?.FileUrl,
                account = new AccountResponse
                {
                    Id = renter.Account.Id,
                    Fullname = renter.Account.Fullname,
                    Role = renter.Account.Role,
                    Username = renter.Account.Username,

                },
                documents = listDoc.Select(a => new DocumentRenterDetailResponse
                {
                    Id = a.Id,
                    DocumentNumber = a.DocumentNumber,
                    DocumentType = a.DocumentType,
                    ExpiryDate = a.ExpiryDate,
                    IssueDate = a.IssueDate,
                    IssuingAuthority = a.IssuingAuthority,
                    RenterId = a.RenterId,
                    VerificationStatus = a.VerificationStatus,
                    VerifiedAt = a.VerifiedAt,
                    Images=mediaDict.TryGetValue(a.Id, out var docsMedia) ? 
                    docsMedia.Select(m => new DocumentMediaResponse
                    {
                        Id = m.Id,
                        fileUrl = m.FileUrl,
                    }).ToList() : new List<DocumentMediaResponse>()
                }

                ).ToList()

            };
            return ResultResponse<RenterDetailResponse>.SuccessResult("Renter found",response);

        }
        catch(Exception ex) 
        {
            return ResultResponse<RenterDetailResponse>.Failure($"An error occurred: {ex.Message}");

        }
    }

   


    public async Task<ResultResponse<CreateStaffAccountResponse>> CreateManagerAccount(CreateManagerRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // 1. Validate username uniqueness
            var existingAccount = await _unitOfWork.GetAccountRepository()
                .GetByUsernameAsync(request.Username);

            if (existingAccount != null)
            {
                return ResultResponse<CreateStaffAccountResponse>.Failure(
                    "Username already exists");
            }

            // 2. Validate branch exists
            var branch = await _unitOfWork.GetBranchRepository()
                .FindByIdAsync(request.BranchId);

            if (branch == null)
            {
                return ResultResponse<CreateStaffAccountResponse>.Failure(
                    "Branch not found");
            }

            // 3. Hash password using injected IPasswordHasher
            var passwordHash = _passwordHasher.Hash(request.Password);

            // 4. Create Account with Staff (Manager role)
            var newAccount = new Account
            {
                Username = request.Username,
                Fullname = request.Fullname,
                Password = passwordHash,
                Role = UserRoleName.MANAGER.ToString(),
                Staff = new Staff
                {
                    BranchId = request.BranchId
                }
            };

            // 5. Save to database
            await _unitOfWork.GetAccountRepository().AddAsync(newAccount);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            // 6. Prepare response
            var response = new CreateStaffAccountResponse
            {
                Id = newAccount.Id,
                Username = newAccount.Username,
                Fullname = newAccount.Fullname,
                Role = newAccount.Role,
                BranchId = newAccount.Staff.BranchId,
                CreatedAt = newAccount.CreatedAt
            };

            return ResultResponse<CreateStaffAccountResponse>.SuccessResult(
                "Manager account created successfully", response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<CreateStaffAccountResponse>.Failure(
                $"An error occurred: {ex.Message}");
        }
    }


    //Test
    public async Task<ResultResponse<CreateAccountResponse>> CreateAccountForTesting(AccountCreateRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Validate role
            var validRoles = new[] { "ADMIN", "MANAGER", "STAFF", "TECHNICIAN", "RENTER" };
            if (!validRoles.Contains(request.Role.ToUpper()))
            {
                return ResultResponse<CreateAccountResponse>.Failure(
                    $"Invalid role. Valid roles are: {string.Join(", ", validRoles)}");
            }

            // Check if username already exists
            var existingAccount = await _unitOfWork.GetAccountRepository()
                .Query()
                .Where(a => a.Username.ToLower() == request.Username.ToLower())
                .FirstOrDefaultAsync();

            if (existingAccount != null)
            {
                return ResultResponse<CreateAccountResponse>.Failure("Username already exists");
            }

            // Hash password
            var hashedPassword = _passwordHasher.Hash(request.Password);

            // Create Account
            var account = new Account
            {
                Username = request.Username,
                Password = hashedPassword,
                Role = request.Role.ToUpper(),
                Fullname = request.Fullname ?? $"Test {request.Role}"
            };

            await _unitOfWork.GetAccountRepository().AddAsync(account);
            await _unitOfWork.SaveChangesAsync();

            Guid? renterId = null;
            Guid? staffId = null;

            // Create role-specific entity
            switch (request.Role.ToUpper())
            {
                case "STAFF":
                case "MANAGER":
                case "TECHNICIAN":
                    var staff = await CreateStaffForAccount(account.Id, request);
                    staffId = staff.Id;
                    break;

                case "ADMIN":
                    var adminStaff = await CreateStaffForAccount(account.Id, request);
                    staffId = adminStaff.Id;
                    break;
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            var response = new CreateAccountResponse
            {
                Id = account.Id,
                Username = account.Username,
                Role = account.Role,
                Fullname = account.Fullname,
                CreatedAt = account.CreatedAt,
                RenterId = renterId,
                StaffId = staffId
            };

            return ResultResponse<CreateAccountResponse>.SuccessResult(
                "Account created successfully for testing", response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<CreateAccountResponse>.Failure(
                $"Error creating account: {ex.Message}");
        }
    }


        private async Task<Staff> CreateStaffForAccount(Guid accountId, AccountCreateRequest request)
        {
            Guid? branchId = request.BranchId;

            // If no branch specified and role is not TECHNICIAN, get first available branch
            if (branchId == null && request.Role.ToUpper() != "TECHNICIAN" && request.Role.ToUpper() != "ADMIN")
            {
                var branch = await _unitOfWork.GetBranchRepository()
                    .Query()
                    .FirstOrDefaultAsync();

                branchId = branch?.Id;
            }

            var staff = new Staff
            {
                AccountId = accountId,
                BranchId = branchId // TECHNICIAN will have null BranchId
            };

            await _unitOfWork.GetStaffRepository().AddAsync(staff);

            return staff;
        }

}
