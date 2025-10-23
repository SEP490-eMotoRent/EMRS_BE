using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class Transaction:BaseEntity
    {
        public string TransactionType { get; set; } 
        public decimal Amount { get; set; }
        public Guid DocNo { get; set; }
        public string Status { get; set; }
     
    }
}
