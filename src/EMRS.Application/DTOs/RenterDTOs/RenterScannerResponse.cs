using EMRS.Application.DTOs.AccountDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RenterDTOs;

public class RenterScannerResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string phone { get; set; }
    public string Address { get; set; }
    public string? DateOfBirth { get; set; }
    public string AvatarUrl { get; set; }
    public string FaceScanUrl { get; set; }
    public AccountResponse account { get; set; }
}
