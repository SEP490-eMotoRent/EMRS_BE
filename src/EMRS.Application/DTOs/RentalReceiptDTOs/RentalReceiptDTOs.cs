using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs
{
    public class ReturnInitResponse
    {
        public Guid BookingId { get; set; }
        public Guid RenterId { get; set; }
        public string RenterName { get; set; }
        public string RenterPhone { get; set; }
        public string RenterEmail { get; set; }
        public string FaceScanUrl { get; set; }

        // Vehicle Info
        public Guid VehicleId { get; set; }
        public string LicensePlate { get; set; }
        public string VehicleModelName { get; set; }
        public string VehicleColor { get; set; }

        // Handover Info
        public Guid RentalReceiptId { get; set; }
        public decimal StartOdometerKm { get; set; }
        public decimal StartBatteryPercentage { get; set; }
        public DateTimeOffset HandoverTime { get; set; }
        public List<string> HandoverImageUrls { get; set; }
        public string HandoverChecklistUrl { get; set; }

        // Booking Info
        public DateTimeOffset StartDatetime { get; set; }
        public DateTimeOffset EndDatetime { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal TotalRentalFee { get; set; }
    }
}
