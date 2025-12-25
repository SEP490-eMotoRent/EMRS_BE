using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.Helper;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountSeedController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountSeedController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private static DateTimeOffset GetVietnamTime()
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, vietnamTimeZone);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateAccountSeedData()
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var currentTime = GetVietnamTime();

                // ================== LẤY 2 BRANCHES ==================
                var branches = await _unitOfWork.GetBranchRepository()
                    .Query()
                    .OrderBy(b => b.BranchName)
                    .ToListAsync();

                if (branches.Count < 2)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Please run DataSeedController first to create branches"
                    });
                }

                var branch1 = branches[0]; // Chi nhánh Quận 1
                var branch3 = branches[1]; // Chi nhánh Quận 3

                // ================== LẤY MEMBERSHIP THẤP NHẤT ==================
                var lowestMembership = await _unitOfWork.GetMembershipRepository()
                    .FindLowestMinBookingMembershipAsync();

                if (lowestMembership == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No membership found. Please create default membership first"
                    });
                }

                // ================== MẬT KHẨU CHUNG ==================
                var passwordHash = PasswordHasher.Hash("123456");

                // ================== 2. TẠO 2 STAFF (MỖI CHI NHÁNH 1) ==================
                var staff1 = new Account
                {
                    Username = "phuuthanh1",
                    Password = passwordHash,
                    Fullname = "Phùng Hữu Thành",
                    Role = UserRoleName.STAFF.ToString(),
                    Staff = new Staff
                    {
                        BranchId = branch1.Id // ✅ Chi nhánh Quận 1
                    }
                };

                var staff3 = new Account
                {
                    Username = "phuuthanh3",
                    Password = passwordHash,
                    Fullname = "Phùng Văn Hậu",
                    Role = UserRoleName.STAFF.ToString(),
                    Staff = new Staff
                    {
                        BranchId = branch3.Id // ✅ Chi nhánh Quận 3
                    }
                };

                await _unitOfWork.GetAccountRepository().AddAsync(staff1);
                await _unitOfWork.GetAccountRepository().AddAsync(staff3);
                await _unitOfWork.SaveChangesAsync();

                // ================== 3. TẠO 2 MANAGER (MỖI CHI NHÁNH 1) ==================
                var manager1 = new Account
                {
                    Username = "manager1",
                    Password = passwordHash,
                    Fullname = "Nguyễn Văn Manager 1",
                    Role = UserRoleName.MANAGER.ToString(),
                    Staff = new Staff
                    {
                        BranchId = branch1.Id // ✅ Chi nhánh Quận 1
                    }
                };

                var manager3 = new Account
                {
                    Username = "manager3",
                    Password = passwordHash,
                    Fullname = "Trần Thị Manager 3",
                    Role = UserRoleName.MANAGER.ToString(),
                    Staff = new Staff
                    {
                        BranchId = branch3.Id // ✅ Chi nhánh Quận 3
                    }
                };

                await _unitOfWork.GetAccountRepository().AddAsync(manager1);
                await _unitOfWork.GetAccountRepository().AddAsync(manager3);
                await _unitOfWork.SaveChangesAsync();

                // ================== 4. TẠO 1 ADMIN ==================
                var admin1 = new Account
                {
                    Username = "admin1",
                    Password = passwordHash,
                    Fullname = "Administrator",
                    Role = UserRoleName.ADMIN.ToString(),
                    Staff = new Staff
                    {
                        BranchId = null // ✅ Admin không thuộc chi nhánh cụ thể
                    }
                };

                await _unitOfWork.GetAccountRepository().AddAsync(admin1);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();

                // ================== RESPONSE CHI TIẾT ==================
                return Ok(new
                {
                    success = true,
                    message = "Account seed data generated successfully",
                    vietnamTime = currentTime,
                    password = "123456", // ✅ Mật khẩu chung
                    data = new
                    {
                        renters = new[]
                        {
                            new { username = "buiphuocloc", role = "RENTER", wallet = "1,000,000đ" },
                            new { username = "lamtanphu", role = "RENTER", wallet = "1,500,000đ" }
                        },
                        staff = new[]
                        {
                            new { username = "phuuhuuthanh1", role = "STAFF", branch = branch1.BranchName },
                            new { username = "phuuhuuthanh3", role = "STAFF", branch = branch3.BranchName }
                        },
                        managers = new[]
                        {
                            new { username = "manager1", role = "MANAGER", branch = branch1.BranchName },
                            new { username = "manager3", role = "MANAGER", branch = branch3.BranchName }
                        },
                        admin = new[]
                        {
                            new { username = "admin1", role = "ADMIN", branch = "N/A (Global)" }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return BadRequest(new
                {
                    success = false,
                    message = $"Error generating account seed data: {ex.Message}",
                    stackTrace = ex.StackTrace
                });
            }
        }

        // ================== ENDPOINT XÓA TẤT CẢ ACCOUNTS (CHO TESTING) ==================
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearAllAccounts()
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var accounts = await _unitOfWork.GetAccountRepository().GetAllAsync();

                foreach (var account in accounts)
                {
                    _unitOfWork.GetAccountRepository().Delete(account);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = "All accounts deleted successfully",
                    deletedCount = accounts.Count
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return BadRequest(new
                {
                    success = false,
                    message = $"Error clearing accounts: {ex.Message}"
                });
            }
        }
    }
}