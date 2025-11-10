using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.ChargingRecordDTOs
{
    public class ChargingRateResponse
    {
        public DateTime ChargingDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty; // "Peak", "Normal", "OffPeak"
        public decimal RatePerKwh { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
