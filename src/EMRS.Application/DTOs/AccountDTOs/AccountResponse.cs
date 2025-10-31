using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.AccountDTOs;

public class AccountResponse
{
  
    public Guid Id { get; set; }
    public string Username { get; set; }

   

    
    public string Role { get; set; }

    public string? Fullname { get; set; }
}
