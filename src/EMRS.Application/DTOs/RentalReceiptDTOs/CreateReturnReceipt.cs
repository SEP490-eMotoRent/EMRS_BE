using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs
{
    public class CreateReturnReceipt
    {
        public Guid BookingId { get; set; }

        public DateTime ActualReturnDatetime { get; set; }
        public decimal EndOdometerKm { get; set; }
        public decimal EndBatteryPercentage { get; set; }
        public string Notes { get; set; }
        public string ReturnImageUrls { get; set; } // ["url1","url2","url3","url4"]
        public IFormFile? ChecklistImage { get; set; }

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
        public List<DamageDetail>? DamageDetails { get; set; }

        public decimal CleaningFee { get; set; }

        public decimal LateReturnFee { get; set; }
        public LateReturnDetails? LateReturnDetails { get; set; }

        public decimal CrossBranchFee { get; set; }
        public CrossBranchDetails? CrossBranchDetails { get; set; }

        public decimal ExcessKmFee { get; set; }
        public ExcessKmDetails? ExcessKmDetails { get; set; }
    }
}
