using EMRS.Application.Common;
using EMRS.Application.DTOs.AccountDTOs;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services
{
    public interface IAuthorizationService
    {
        Task<ResultResponse<Account>> RegisterUser(RegisterUserRequest registerUserRequest);
    }
}
