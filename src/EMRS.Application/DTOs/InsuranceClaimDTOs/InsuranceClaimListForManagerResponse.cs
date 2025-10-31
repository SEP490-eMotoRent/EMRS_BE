using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.InsuranceClaimDTOs
{
    public class InsuranceClaimListForManagerResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? IncidentDate { get; set; }
        public string IncidentLocation { get; set; } = null!;

        // Renter Basic Info
        public string RenterName { get; set; } = null!;
        public string RenterPhone { get; set; } = null!;

        // Vehicle Basic Info
        public string VehicleModelName { get; set; } = null!;
        public string LicensePlate { get; set; } = null!;

        // Booking Info
        public Guid BookingId { get; set; }
        public string HandoverBranchName { get; set; } = null!;

        public DateTimeOffset CreatedAt { get; set; }
    }
}
