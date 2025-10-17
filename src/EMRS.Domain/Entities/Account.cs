using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities;
public partial class Account: BaseEntity
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    public string? Fullname { get; set; }

    [MaxLength(255)]
    public string? RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenExpiry { get; set; }
    public bool IsRefreshTokenRevoked { get; set; } = false;

 
    [MaxLength(100)]
    public string? ResetPasswordToken { get; set; }
    public DateTimeOffset? ResetPasswordTokenExpiry { get; set; }


    

    public Renter? Renter { get; set; }

    public Staff? Staff { get; set; }




    public bool IsAdmin() => Role == "Admin";

    public bool IsStaff() => Role == "Staff";

    public bool IsManager() => Role == "Manager";

    public bool IsRenter() => Role == "Renter";

    public bool IsTechnician() => Role == "Technician";
}
