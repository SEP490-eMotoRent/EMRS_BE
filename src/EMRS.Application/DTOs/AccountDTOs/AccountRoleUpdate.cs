using EMRS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.AccountDTOs
{
    public class AccountRoleUpdate
    {
        public Guid Id { get; set; }
        public UserRoleName Role { get; set; }
    }
}
