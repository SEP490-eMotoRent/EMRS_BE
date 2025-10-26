using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.AccountDTOs;

public class RenterAccountUpdateRequest
{

    public string Email { get; set; }
    public string phone { get; set; }
    public string Address { get; set; }
    public string? DateOfBirth { get; set; }
    public Guid? MediaId { get; set; } = null;
    public IFormFile? ProfilePicture { get; set; }
    public string? Fullname { get; set; }
}
