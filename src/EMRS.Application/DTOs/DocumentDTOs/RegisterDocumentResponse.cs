using EMRS.Application.DTOs.RenterDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.DocumentDTOs;

public class RegisterDocumentResponse
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
    public string fileUrl { get; set; }
}
