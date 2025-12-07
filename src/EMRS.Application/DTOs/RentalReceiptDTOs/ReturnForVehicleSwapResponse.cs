using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs
{
    public class ReturnForVehicleSwapResponse
    {
        public Guid BookingId { get; set; }
        public Guid RentalReceiptId { get; set; }
        public string BookingStatus { get; set; } // Vẫn "Renting"
        public Guid OldVehicleId { get; set; }
        public string OldVehicleLicensePlate { get; set; }
        public decimal TotalKmDriven { get; set; } // Km của xe cũ
        public string Message { get; set; } // "Ready for vehicle swap"
    }
}
