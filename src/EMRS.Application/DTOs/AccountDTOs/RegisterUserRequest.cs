using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.AccountDTOs;

public class RegisterUserRequest
{

    public string phone { get; set; }
    public string Address { get; set; }
    public string DateOfBirth { get; set; }
    public string AvatarUrl { get; set; }


    public string Email { get; set; }
    //renter

    public string Username { get; set; }

    public string Fullname { get; set; }
    public string Password { get; set; } 

   


 


}
