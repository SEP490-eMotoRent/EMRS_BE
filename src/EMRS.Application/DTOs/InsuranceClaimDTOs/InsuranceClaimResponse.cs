using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.InsuranceClaimDTOs
{
    public class InsuranceClaimResponse
    {
        public Guid Id { get; set; }
        public DateTime? IncidentDate { get; set; }
        public string IncidentLocation { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = null!;
        public Guid BookingId { get; set; }
        public Guid RenterId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
