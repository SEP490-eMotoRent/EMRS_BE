using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions.Models.ZaloPay
{
    public class ZaloPayCallbackResponseData
    {
        public int AppId { get; set; }
        public string AppTransId { get; set; }
        public int PmcId { get; set; }
        public string BankCode { get; set; }
        public long Amount { get; set; }
        public long DiscountAmount { get; set; }
        public int Status { get; set; }
        public string Checksum { get; set; }


        public bool IsSuccess => Status == 1;
        public string Message { get; set; }
    }
}
