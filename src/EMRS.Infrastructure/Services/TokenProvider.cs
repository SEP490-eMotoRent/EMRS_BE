using EMRS.Application.Abstractions;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Services;

public class TokenProvider():ITokenProvider
{
    public string JWTGenerator(Account account)
    {
        string SecretKey= Environment.GetEnvironmentVariable("SECRET_KEY");
        var SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));

        var credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);


        Guid? userId = account.Role == UserRoleName.RENTER.ToString()
          ? account.Renter?.Id
          : account.Staff?.Id;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim("Id",account.Id.ToString()),
                new System.Security.Claims.Claim("UserId", userId.ToString()),
                new System.Security.Claims.Claim("Username", account.Username),
                new System.Security.Claims.Claim("Role", account.Role)
            }),
            Expires = DateTime.UtcNow.AddMinutes(24*60*7),
            SigningCredentials = credentials,
            Issuer= Environment.GetEnvironmentVariable("ISSUER"),
            Audience= Environment.GetEnvironmentVariable("AUDIENCE")
        };
        var tokenHandler = new JsonWebTokenHandler();
        var token= tokenHandler.CreateToken(tokenDescriptor);
        return token;
    }
}
