using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.TransactionDTOs
{
    public class TransactionTotalResponsePerDay
    {
        public DateOnly Date { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal RefundAmount { get; set; }
    }
}
