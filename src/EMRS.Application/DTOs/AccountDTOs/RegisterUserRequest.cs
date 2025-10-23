using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.AccountDTOs;

public class RegisterUserRequest
{

    public string phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DateOfBirth { get; set; }=string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;


    public string Email { get; set; }
    //renter

    public string Username { get; set; }

    public string Fullname { get; set; } = string.Empty;
    public string Password { get; set; } 

    


 


}
