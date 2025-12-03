using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs
{
    public class CrossBranchDetails
    {
        public Guid HandoverBranchId { get; set; }
        public string HandoverBranchName { get; set; }
        public Guid ReturnBranchId { get; set; }
        public string ReturnBranchName { get; set; }
    }
}
