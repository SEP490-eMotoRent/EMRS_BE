using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions.Models.ZaloPay
{
    public class ZaloPayResponse
    {
        public int returncode { get; set; }
        public string returnmessage { get; set; }
        public string zptranstoken { get; set; }
        public string orderurl { get; set; }
    }

}
