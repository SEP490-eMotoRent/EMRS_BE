using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs
{
    public class CreateReturnReceiptRequest
    {
        public Guid BookingId { get; set; }
        public decimal EndOdometerKm { get; set; }
        public decimal EndBatteryPercentage { get; set; }
        public string Notes { get; set; }

        public List<string> ReturnImageUrls { get; set; }

        // Checklist (upload as image or JSON)
        public IFormFile ChecklistImage { get; set; }

        // Additional Fees
        public List<AdditionalFeeInput> AdditionalFees { get; set; }
    }

    public class AdditionalFeeInput
    {
        public string FeeType { get; set; } // "DAMAGE", "CLEANING", "LATE_RETURN", "CROSS_BRANCH", "EXCESS_KM"
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }

    public class CreateReturnReceiptResponse
    {
        public Guid BookingId { get; set; }
        public Guid RentalReceiptId { get; set; }

        // Settlement Summary
        public SettlementSummary Settlement { get; set; }
    }

    public class SettlementSummary
    {
        public decimal BaseRentalFee { get; set; }
        public decimal TotalChargingFee { get; set; }
        public decimal TotalAdditionalFees { get; set; }
        public AdditionalFeesBreakdown FeesBreakdown { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal RefundAmount { get; set; } // Positive = hoàn tiền, Negative = phải trả thêm
    }

    public class AdditionalFeesBreakdown
    {
        public decimal DamageFee { get; set; }
        public decimal CleaningFee { get; set; }
        public decimal LateReturnFee { get; set; }
        public decimal CrossBranchFee { get; set; }
        public decimal ExcessKmFee { get; set; }
    }
}
