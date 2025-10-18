using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial  class WithdrawalRequest:BaseEntity
    {
        public decimal Amount { get; set; }
        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountName { get; set; }
        public string Status { get; set; }
     
        public DateTime? ProcessedAt { get; set; }
        public string RejectionReason { get; set; }
        public Guid WalletId { get; set; }

        //relationship
        [ForeignKey(nameof(WalletId))]
        public Wallet Wallet { get; set; } = null!;
    }
}
