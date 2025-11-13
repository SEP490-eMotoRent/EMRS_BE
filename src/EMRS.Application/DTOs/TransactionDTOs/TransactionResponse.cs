using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.TransactionDTOs
{
    public class TransactionResponse
    {
        public Guid Id { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public Guid DocNo { get; set; }
        public string Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
