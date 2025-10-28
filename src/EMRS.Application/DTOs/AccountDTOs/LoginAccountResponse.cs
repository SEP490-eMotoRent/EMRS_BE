using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.AccountDTOs;

public class LoginAccountResponse
{
    public string AccessToken { get; set; }
    public User User { get; set; }
}
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string AvatarUrl { get; set; }
    public string Role { get; set; }
    public string FullName { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? BranchId { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]

    public string? BranchName { get; set; }

}
