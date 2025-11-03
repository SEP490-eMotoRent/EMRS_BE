using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs
{
    public class UpdateReturnReceiptRequest
    {
        public Guid BookingId { get; set; }
        public decimal? EndOdometerKm { get; set; }
        public decimal? EndBatteryPercentage { get; set; }
        public string? Notes { get; set; }
        public List<IFormFile>? NewReturnImages { get; set; }
        public IFormFile? NewChecklistImage { get; set; }

        public List<AdditionalFeeInput>? NewAdditionalFees { get; set; }
        public bool RerunAIAnalysis { get; set; } = false;
    }
}
