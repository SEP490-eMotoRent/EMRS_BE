using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.WalletDTOs
{
    public class WalletTopUpResponse
    {
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string VNPayUrl { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
