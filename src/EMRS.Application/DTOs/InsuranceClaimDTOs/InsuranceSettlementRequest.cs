using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.InsuranceClaimDTOs
{
    public class InsuranceSettlementRequest
    {
        public decimal VehicleDamageCost { get; set; }
        public decimal PersonInjuryCost { get; set; }
        public decimal ThirdPartyCost { get; set; }
        public decimal InsuranceCoverageAmount { get; set; }
        public IFormFile? InsuranceClaimPdfFile { get; set; }
    }
}
