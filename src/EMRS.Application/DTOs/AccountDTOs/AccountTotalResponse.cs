using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.AccountDTOs
{
    public class AccountTotalResponse
    {
        public int TotalAccounts { get; set; }
        public int TotalAdmin { get; set; }
        public int TotalStaff { get; set; }
        public int TotalManager { get; set; }
        public int TotalRenter { get; set; }
        public int TotalTechnician { get; set; }
    }
}
