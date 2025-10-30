using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.InsuranceClaimDTOs
{
    public class InsuranceClaimDetailResponse
    {
        public Guid Id { get; set; }
        public DateTime? IncidentDate { get; set; }
        public string IncidentLocation { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = null!;
        public decimal TotalCost { get; set; }
        public decimal InsuranceCoverageAmount { get; set; }
        public decimal RenterLiabilityAmount { get; set; }
        public Guid BookingId { get; set; }
        public Guid RenterId { get; set; }
        public List<string> IncidentImages { get; set; } = new();
        public DateTimeOffset CreatedAt { get; set; }
    }
}
