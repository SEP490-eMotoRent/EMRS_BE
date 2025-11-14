using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.WithdrawalRequestDTOs
{
    public class WithdrawalRequestRejectRequest
    {
        public Guid WithdrawalRequestId { get; set; }
        public string RejectionReason { get; set; }
    }
}
