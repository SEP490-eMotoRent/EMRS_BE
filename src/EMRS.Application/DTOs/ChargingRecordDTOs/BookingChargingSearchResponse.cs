using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.ChargingRecordDTOs
{
    public class BookingChargingSearchResponse
    {
        public Guid BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string BookingStatus { get; set; } = string.Empty;
        public string RenterFullName { get; set; } = string.Empty;
        public string VehicleModelName { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string BranchAddress { get; set; } = string.Empty;
        public decimal BatteryAtHandover { get; set; }
        public DateTime? LastChargingDate { get; set; }
    }

}
