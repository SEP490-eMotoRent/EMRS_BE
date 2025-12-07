using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs
{
    public class ReturnForVehicleSwapRequest
    {
        public Guid BookingId { get; set; }
        public Guid RentalReceiptId { get; set; } 
        public decimal EndOdometerKm { get; set; }
        public decimal EndBatteryPercentage { get; set; }
        public string? Notes { get; set; }
        public string ReturnImageUrls { get; set; }
        public IFormFile? ChecklistImage { get; set; }
    }
}
