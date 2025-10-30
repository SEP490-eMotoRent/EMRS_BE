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

public class AccountDetailResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;


   


    public string Role { get; set; } = string.Empty;

    public string? Fullname { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RenterResponse renter { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StaffResponse staff { get; set; }


}
