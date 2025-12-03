using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs
{
    public class LateReturnDetails
    {
        public DateTimeOffset EndDatetime { get; set; }
        public DateTimeOffset ActualReturnDatetime { get; set; }
        public double LateHours { get; set; }
        public decimal RatePerHour { get; set; }
    }
}
