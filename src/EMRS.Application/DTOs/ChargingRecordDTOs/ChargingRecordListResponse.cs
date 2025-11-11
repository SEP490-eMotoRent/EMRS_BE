using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.ChargingRecordDTOs
{
    public class ChargingRecordListResponse
    {
        public Guid Id { get; set; }
        public DateTime ChargingDate { get; set; }
        public decimal StartBatteryPercentage { get; set; }
        public decimal EndBatteryPercentage { get; set; }
        public decimal BatteryPercentageCharged { get; set; }
        public decimal KwhCharged { get; set; }
        public decimal RatePerKwh { get; set; }
        public decimal Fee { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Thông tin Booking
        public string BookingCode { get; set; } = string.Empty;
        public string VehicleModelName { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;

        // Thông tin Branch
        public string BranchName { get; set; } = string.Empty;
        public string BranchAddress { get; set; } = string.Empty;

        // Thông tin Staff
        public string StaffName { get; set; } = string.Empty;
    }
}
