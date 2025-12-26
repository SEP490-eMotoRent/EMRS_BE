using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.TransactionDTOs
{
    public class TransactionTotalResponsePerYear
    {
        public int Year { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
