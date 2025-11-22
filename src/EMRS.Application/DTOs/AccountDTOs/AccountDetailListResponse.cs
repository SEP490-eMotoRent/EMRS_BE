using EMRS.Application.DTOs.RenterDTOs;
using EMRS.Application.DTOs.StaffDTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.AccountDTOs;

public class AccountDetailListResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;


   


    public string Role { get; set; } = string.Empty;

    public string? Fullname { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RenterAccountDetailResponse renter { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StaffAccountDetailResponse staff { get; set; }


}
public class  RenterAccountDetailResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string phone { get; set; }
    public string Address { get; set; }
    public string? DateOfBirth { get; set; }
    public bool IsVerified { get; set; } = false;
}
public class StaffAccountDetailResponse
{
    public Guid Id { get; set; }
  
}
