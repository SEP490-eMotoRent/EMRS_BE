using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.AccountDTOs
{
    public class CreateStaffAccountResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public Guid? BranchId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
