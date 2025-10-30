using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.InsuranceClaimDTOs
{
    public class UpdateInsuranceClaimRequest
    {
        public DateTime? IncidentDate { get; set; }
        public string? IncidentLocation { get; set; }
        public string? Description { get; set; }
        public string? Severity { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public string? RejectionReason { get; set; }
        public List<IFormFile?>? AdditionalImageFiles { get; set; }
    }
}
