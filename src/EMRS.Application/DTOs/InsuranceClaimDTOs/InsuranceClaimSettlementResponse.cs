using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.InsuranceClaimDTOs
{
    public class InsuranceClaimSettlementResponse : InsuranceClaimForManagerResponse
    {
        public decimal VehicleDamageCost { get; set; }
        public decimal PersonInjuryCost { get; set; }
        public decimal ThirdPartyCost { get; set; }
        public decimal TotalCost { get; set; }
        public decimal InsuranceCoverageAmount { get; set; }
        public decimal RenterLiabilityAmount { get; set; }
        public string? InsuranceClaimPdfUrl { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
