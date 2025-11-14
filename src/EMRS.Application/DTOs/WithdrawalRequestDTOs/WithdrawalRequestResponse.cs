using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.WithdrawalRequestDTOs
{
    public class WithdrawalRequestResponse
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountName { get; set; }
        public string Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? RejectionReason { get; set; }
    }
}
