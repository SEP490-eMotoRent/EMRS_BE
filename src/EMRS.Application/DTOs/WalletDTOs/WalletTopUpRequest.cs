using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.WalletDTOs
{
    public class WalletTopUpRequest
    {
        //[Required(ErrorMessage = "Amount is required")]
        //[Range(10000, double.MaxValue, ErrorMessage = "Amount must be at least 10,000 VND")]
        public decimal Amount { get; set; }
    }
}
