using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.TransactionDTOs
{
    public class TransactionTotalResponseMonths
    {
        public List<TransactionMonthTotal>  monthTotals { get; set; }
    }
    public class TransactionMonthTotal
    {
        public int Month { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
