using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.AccountDTOs;

public class LoginAccountRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}
