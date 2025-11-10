using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.ChargingRecordDTOs
{
    public class ChargingRecordResponse
    {
        public Guid Id { get; set; }
        public DateTime ChargingDate { get; set; }
        public decimal StartBatteryPercentage { get; set; }
        public decimal EndBatteryPercentage { get; set; }
        public decimal BatteryPercentageCharged { get; set; }
        public decimal KwhCharged { get; set; }
        public decimal RatePerKwh { get; set; }
        public decimal Fee { get; set; }
        public string TimeSlot { get; set; } = string.Empty; // "Peak", "Normal", "OffPeak"
        public string Notes { get; set; } = string.Empty;
    }
}
