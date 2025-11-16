using EMRS.Application.DTOs.AuthDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions.Models
{
    public interface IGoogleAuthService
    {
        Task<GoogleLoginResponse> GoogleLoginAsync(string idToken);
    }
}
