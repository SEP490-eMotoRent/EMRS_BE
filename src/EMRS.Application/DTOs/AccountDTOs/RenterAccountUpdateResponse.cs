using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.AccountDTOs;

public class RenterAccountUpdateResponse
{
    public string Email { get; set; }
    public string phone { get; set; }
    public string Address { get; set; }
    public string? DateOfBirth { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Fullname { get; set; }
}
