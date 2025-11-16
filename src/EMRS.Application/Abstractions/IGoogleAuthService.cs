using EMRS.Application.Common;
using EMRS.Application.DTOs.AccountDTOs;
using EMRS.Application.DTOs.AuthDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions
{
    public interface IGoogleAuthService
    {
        Task<ResultResponse<LoginAccountResponse>> GoogleLoginAsync(GoogleLoginRequest request);
    }
}
