using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.DocumentDTOs;

public class DocumentCreateRequest
{
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? IssuingAuthority { get; set; }

    public Guid RenterId {  get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public DateTime? VerifiedAt { get; set; }

    public IFormFile DocumentFile { get; set; }
}
