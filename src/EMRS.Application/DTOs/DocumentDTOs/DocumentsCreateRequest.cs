using EMRS.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.DocumentDTOs;

public class DocumentsCreateRequest
{
    public string DocumentNumber { get; set; } 
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? IssuingAuthority { get; set; }

    public string VerificationStatus { get; set; } 
    public DateTime? VerifiedAt { get; set; }

    public IFormFile FrontDocumentFile { get; set; }
    public IFormFile BackDocumentFile { get; set; }
}


