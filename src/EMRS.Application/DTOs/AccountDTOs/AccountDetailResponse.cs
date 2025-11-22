using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.DocumentDTOs;
using EMRS.Application.DTOs.RenterDTOs;
using EMRS.Application.DTOs.StaffDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.AccountDTOs;

public class AccountDetailResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;





    public string Role { get; set; } = string.Empty;

    public string? Fullname { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RenterAccountDetailNoListResponse renter { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StaffResponse staff { get; set; }


}
public class RenterAccountDetailNoListResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string phone { get; set; }
    public string Address { get; set; }
    public string? DateOfBirth { get; set; }
    public bool IsVerified { get; set; } = false;
    public string AvatarUrl { get; set; }
    public List<DocumentRenterAccountDetailResponse> documents { get; set; }
}
public class DocumentRenterAccountDetailResponse
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; }
    public string DocumentNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? IssuingAuthority { get; set; }

    public string VerificationStatus { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid RenterId { get; set; }
    public List<DocumentMediaResponse> Images { get; set; }
}
public class StaffAccountDetailNoListResponse
{
    public Guid Id { get; set; }
    public BranchResponse? branch { get; set; }
}
