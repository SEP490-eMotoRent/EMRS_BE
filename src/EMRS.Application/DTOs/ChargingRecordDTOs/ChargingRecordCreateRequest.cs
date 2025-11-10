using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.ChargingRecordDTOs
{
    public class ChargingRecordCreateRequest
    {
        public Guid BookingId { get; set; }
        public DateTime ChargingDate { get; set; }
        public decimal StartBatteryPercentage { get; set; }
        public decimal EndBatteryPercentage { get; set; }
        public decimal KwhCharged { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

}
