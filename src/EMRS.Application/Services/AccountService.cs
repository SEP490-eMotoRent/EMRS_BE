using EMRS.Application.Abstractions;
using EMRS.Application.Abstractions.Models.FacePlusPlus;
using EMRS.Application.Common;
using EMRS.Application.DTOs.AccountDTOs;
using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.DocumentDTOs;
using EMRS.Application.DTOs.MediaDTOs;
using EMRS.Application.DTOs.MembershipDTOs;
using EMRS.Application.DTOs.RenterDTOs;
using EMRS.Application.DTOs.StaffDTOs;
using EMRS.Application.Helper;
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
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IFacePlusPlusService _facePlusPlusClient;
    public AccountService(IFacePlusPlusService facePlusPlusClient,IUnitOfWork unitOfWork,ICurrentUserService currentUserService,ICloudinaryService cloudinaryService)
    {
        _facePlusPlusClient= facePlusPlusClient;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _cloudinaryService = cloudinaryService;
    }
    public async Task<ResultResponse<AccountResponse>> UpdateAccountRole(AccountRoleUpdate  accountRoleUpdate)
    {
        try
        {
            var account= await _unitOfWork.GetAccountRepository().FindByIdAsync(accountRoleUpdate.Id);
            if(account == null || account.IsDeleted)
            {
                return ResultResponse<AccountResponse>.Failure("Account not found");
            }
            account.Role = accountRoleUpdate.Role.ToString();
            _unitOfWork.GetAccountRepository().Update(account);
            await _unitOfWork.SaveChangesAsync();
            var response = new AccountResponse
            {
                Id = account.Id,
                Fullname = account.Fullname,
                Username = account.Username,
                Role = account.Role
            };
            return ResultResponse<AccountResponse>.SuccessResult("Account role updated successfully", response);

        }
        catch (Exception ex)
        {
            return ResultResponse<AccountResponse>.Failure($"An error occurred: {ex.Message}");
        }
    }
    public async Task<ResultResponse<AccountResponse>> SoftDeleteAccount(AccountDeleteUpdate accountDeleteUpdate)
    {
        try
        {
            var account = await _unitOfWork.GetAccountRepository().FindByIdAsync(accountDeleteUpdate.Id);
            if (account == null || account.IsDeleted)
            {
                return ResultResponse<AccountResponse>.Failure("Account not found");
            }
            account.IsDeleted = accountDeleteUpdate.IsDeleted;
            _unitOfWork.GetAccountRepository().Update(account);
            await _unitOfWork.SaveChangesAsync();
            var response = new AccountResponse
            {
                Id = account.Id,
                Fullname = account.Fullname,
                Username = account.Username,
                Role = account.Role
            };
            return ResultResponse<AccountResponse>.SuccessResult("Account role deleted successfully", response);

        }
        catch (Exception ex)
        {
            return ResultResponse<AccountResponse>.Failure($"An error occurred: {ex.Message}");
        }
    }
    public async Task<ResultResponse<List<AccountDetailListResponse>>> GetAllAccountAsync()
    {
        try {
            var accounts = await _unitOfWork.GetAccountRepository().GetAccountsWithReferenceAsync();
            var roleCheck = UserRoleName.RENTER.ToString();

            var response = accounts.Select(a =>
            {

             

                return new AccountDetailListResponse
                {
                    Id = a.Id,
                    Fullname = a.Fullname,
                    Username = a.Username,
                    Role = a.Role,

                    staff = a.Role != roleCheck && a.Staff != null
                        ? new StaffAccountDetailResponse
                        {
                            Id = a.Staff.Id,
                           
                        }
                        : null,

                    renter = a.Role == roleCheck && a.Renter != null
                        ? new RenterAccountDetailResponse
                        {
                            Id = a.Renter.Id,
                            Email = a.Renter.Email,
                            Address = a.Renter.Address,
                            DateOfBirth = a.Renter.DateOfBirth,
                            phone = a.Renter.phone,
                            IsVerified= a.Renter.IsVerified,
                        }
                        : null
                };
            }).ToList();

            return ResultResponse<List<AccountDetailListResponse>>.SuccessResult("", response);
        }
        catch (Exception ex) {
            return ResultResponse<List<AccountDetailListResponse>>.Failure($"An error occurred: {ex.Message}");
        }
    }
    public async Task<ResultResponse<AccountDetailResponse>> GetAccountDetailAsync(Guid accountId)
    {
        try
        {
            var account = await _unitOfWork
                .GetAccountRepository()
                .GetAccountWithReferenceAsync(accountId);

            if (account == null)
                return ResultResponse<AccountDetailResponse>.Failure("Account not found");

            var roleCheck = UserRoleName.RENTER.ToString();

            var response = new AccountDetailResponse
            {
                Id = account.Id,
                Fullname = account.Fullname,
                Username = account.Username,
                Role = account.Role
            };

          
            if (account.Role != roleCheck && account.Staff != null)
            {
                response.staff = new StaffResponse
                {
                    Id = account.Staff.Id,
                    Branch = account.Staff.Branch != null
                        ? new BranchResponse
                        {
                            Id = account.Staff.Branch.Id,
                            Phone = account.Staff.Branch.Phone,
                            Address = account.Staff.Branch.Address,
                            BranchName = account.Staff.Branch.BranchName,
                            City = account.Staff.Branch.City,
                            ClosingTime = account.Staff.Branch.ClosingTime,
                            Email = account.Staff.Branch.Email,
                            Latitude = account.Staff.Branch.Latitude,
                            Longitude = account.Staff.Branch.Longitude,
                            OpeningTime = account.Staff.Branch.OpeningTime
                        }
                        : null
                };
            }

            if (account.Role == roleCheck && account.Renter != null)
            {
                var renterId = account.Renter.Id;

                var avatarUrl = _unitOfWork.GetMediaRepository()
                    .Query()
                    .Where(x => x.DocNo == renterId && x.EntityType == MediaEntityTypeEnum.Renter.ToString())
                    .Select(x => x.FileUrl)
                    .FirstOrDefault();

                var documents = _unitOfWork.GetDocumentRepository()
                    .Query()
                    .Where(d => d.RenterId == renterId)
                    .ToList();

                var documentResponses = new List<DocumentRenterAccountDetailResponse>();

                foreach (var doc in documents)
                {
                    var docImages = _unitOfWork.GetMediaRepository()
                        .Query()
                        .Where(m => m.DocNo == doc.Id && m.EntityType == MediaEntityTypeEnum.Document.ToString())
                        .Select(m => new DocumentMediaResponse
                        {
                            Id = m.Id,
                            fileUrl = m.FileUrl,
                          
                        })
                        .ToList();

                    documentResponses.Add(new DocumentRenterAccountDetailResponse
                    {
                        Id = doc.Id,
                        DocumentType = doc.DocumentType,
                        DocumentNumber = doc.DocumentNumber,
                        IssueDate = doc.IssueDate,
                        ExpiryDate = doc.ExpiryDate,
                        IssuingAuthority = doc.IssuingAuthority,
                        VerificationStatus = doc.VerificationStatus,
                        VerifiedAt = doc.VerifiedAt,
                        RenterId = renterId,
                        Images = docImages
                    });
                }

                response.renter = new RenterAccountDetailNoListResponse
                {
                    Id = renterId,
                    Email = account.Renter.Email,
                    phone = account.Renter.phone,
                    Address = account.Renter.Address,
                    DateOfBirth = account.Renter.DateOfBirth,
                    IsVerified = account.Renter.IsVerified,
                    AvatarUrl = avatarUrl,
                    documents = documentResponses!=null ? documentResponses : new List<DocumentRenterAccountDetailResponse>()
                };
            }

            return ResultResponse<AccountDetailResponse>.SuccessResult("", response);
        }
        catch (Exception ex)
        {
            return ResultResponse<AccountDetailResponse>.Failure($"An error occurred: {ex.Message}");
        }
    }


    public async Task<ResultResponse<RenterAccountUpdateResponse>> UpdateUserProfile(RenterAccountUpdateRequest renterAccountUpdateRequest)
    {
        try
        {
            var renterId = Guid.Parse(_currentUserService.UserId);
            var renter = await _unitOfWork.GetRenterRepository().GetRenterByRenterIdAsync(renterId);
            var account = await _unitOfWork.GetAccountRepository().FindByIdAsync(renter.AccountId);
            if(account==null|| account.IsDeleted)
            {
                return ResultResponse<RenterAccountUpdateResponse>.Failure("Account not found.");
            }
            var check = await _unitOfWork.GetAccountRepository().GetByEmaiOrUsernameAsync(renterAccountUpdateRequest.Email,account.Username);
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
    public async Task<ResultResponse<RenterScannerResponse>> GetRenterByCitizenIdAsync(string renterCitizenNumber)
    {
        try
        {

            
            Renter renter = await _unitOfWork.GetRenterRepository()
                .GetRenterByCitizenAsync(renterCitizenNumber);
            if (renter == null)
            {
                return ResultResponse<RenterScannerResponse>.Failure("Can't find any renter ");
            }
            Media avatar = await _unitOfWork.GetMediaRepository()
              .GetAMediaWithCondAsync(renter.Id, MediaEntityTypeEnum.Renter.ToString());
         
           
         
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
                FaceScanUrl = null
            };

            return ResultResponse<RenterScannerResponse>.SuccessResult("Renter found", response);

        }
        catch (Exception ex)
        {
            return ResultResponse<RenterScannerResponse>.Failure($"An error occurred: {ex.Message}");
        }
    }
    public async Task<ResultResponse<RenterScannerResponse>> ScanAndReturnRenterInfo(IFormFile image)
    {
        try
        {
            
            FaceSearchResult faceSearchResult = await _facePlusPlusClient.SearchByFileAsync(image);
            if (faceSearchResult == null)
            {
                return ResultResponse<RenterScannerResponse>.Failure("An error occurred while searching for renter face");
            }
            if (faceSearchResult.IsMatch == false)
            {
                return ResultResponse<RenterScannerResponse>.Failure($"{faceSearchResult.Message}");
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
    public async Task<ResultResponse<AccountTotalResponse>> GetDashboardInformationForAccounts()
    {
        try
        {
            var accounts = (await _unitOfWork.GetAccountRepository().GetAllAsync()).Where(a=> !a.IsDeleted).ToList();

            var response = new AccountTotalResponse
            {
                TotalAccounts = accounts.Count,
                TotalAdmin = accounts.Count(a => a.Role == UserRoleName.ADMIN.ToString()),
                TotalStaff = accounts.Count(a => a.Role == UserRoleName.STAFF.ToString()),
                TotalManager = accounts.Count(a => a.Role == UserRoleName.MANAGER.ToString()),
                TotalRenter = accounts.Count(a => a.Role == UserRoleName.RENTER.ToString()),
                TotalTechnician = accounts.Count(a => a.Role == UserRoleName.TECHNICIAN.ToString())
            };

            return ResultResponse<AccountTotalResponse>.SuccessResult(
                "Dashboard account info retrieved successfully", response);
        }
        catch (Exception ex)
        {
            return ResultResponse<AccountTotalResponse>.ServerError(
                $"Error retrieving account dashboard info: {ex.Message}");
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
                Avatar= avatar != null ? new MediaResponse
                {
                    Id = avatar.Id,
                    FileUrl = avatar.FileUrl,
                    DocNo = avatar.DocNo,
                    EntityType = avatar.EntityType,
                    MediaType= avatar.MediaType
                } : null,
                account = new AccountResponse
                {
                    Id = renter.Account.Id,
                    Fullname = renter.Account.Fullname,
                    Role = renter.Account.Role,
                    Username = renter.Account.Username,

                },
                membership= renter.MembershipId != null ? new MembershipResponse
                {
                    Id = renter.Membership.Id,
                    CreatedAt = renter.Membership.CreatedAt,
                    Description = renter.Membership.Description,
                    DiscountPercentage = renter.Membership.DiscountPercentage,
                    MinBookings = renter.Membership.MinBookings,
                    TierName = renter.Membership.TierName,
                    UpdatedAt=renter.Membership.UpdatedAt
                } : null,
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
            var passwordHash = PasswordHasher.Hash(request.Password);

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


    
    public async Task<ResultResponse<CreateAccountResponse>> CreateAccount(AccountCreateRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            
            var validRoles = new[] { "ADMIN", "MANAGER", "STAFF", "TECHNICIAN", "RENTER" };
            if (!validRoles.Contains(request.Role.ToUpper()))
            {
                return ResultResponse<CreateAccountResponse>.Failure(
                    $"Invalid role. Valid roles are: {string.Join(", ", validRoles)}");
            }

           
            var existingAccount = await _unitOfWork.GetAccountRepository()
                .Query()
                .Where(a => a.Username.ToLower() == request.Username.ToLower())
                .FirstOrDefaultAsync();

            if (existingAccount != null)
            {
                return ResultResponse<CreateAccountResponse>.Failure("Username already exists");
            }

           
            var hashedPassword = PasswordHasher.Hash(request.Password);

            
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
