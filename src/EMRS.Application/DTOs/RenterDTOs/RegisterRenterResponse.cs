using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RenterDTOs;

public class RegisterRenterResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string phone { get; set; }
    public string Address { get; set; }
    public string? DateOfBirth { get; set; }
    public Guid MembershipId { get; set; }
    public string Fullname { get; set; }
    public string Username { get; set; }

    public DateTime? VerificationCodeExpiry { get; set; }
}
