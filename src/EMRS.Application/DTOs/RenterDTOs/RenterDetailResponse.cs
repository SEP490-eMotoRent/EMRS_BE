using EMRS.Application.DTOs.AccountDTOs;
using EMRS.Application.DTOs.DocumentDTOs;
using EMRS.Application.DTOs.MembershipDTOs;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RenterDTOs;

public class RenterDetailResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string phone { get; set; }
    public string Address { get; set; }
    public string? DateOfBirth { get; set; }
    public string AvatarUrl { get; set; }
    public AccountResponse account { get; set; }
    public MembershipResponse membership { get; set; }
    public List<DocumentRenterDetailResponse>? documents { get; set; }
}
public class DocumentRenterDetailResponse
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
    public RenterResponse renter { get; set; }
    public List<DocumentMediaResponse> Images { get; set; }
}
public class DocumentMediaResponse
{
    public Guid Id { get; set; }
    public string fileUrl { get; set; }
}
