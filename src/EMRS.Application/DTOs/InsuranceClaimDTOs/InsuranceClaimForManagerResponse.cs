using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.InsuranceClaimDTOs
{
    public class InsuranceClaimForManagerResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? IncidentDate { get; set; }
        public string IncidentLocation { get; set; } = null!;
        public string Description { get; set; } = string.Empty;

        // Renter Info
        public string RenterName { get; set; } = null!;
        public string RenterPhone { get; set; } = null!;
        public string RenterEmail { get; set; } = null!;
        public string Address { get; set; } = string.Empty;

        // Vehicle Info
        public string VehicleModelName { get; set; } = null!;
        public string LicensePlate { get; set; } = null!;
        public string VehicleDescription { get; set; } = string.Empty;

        // Booking Info
        public Guid BookingId { get; set; }
        public string HandoverBranchName { get; set; } = null!;
        public string HandoverBranchAddress { get; set; } = string.Empty;
        public DateTime? BookingStartDate { get; set; }
        public DateTime? BookingEndDate { get; set; }

        // Insurance Package Info
        public string PackageName { get; set; } = null!;
        public decimal PackageFee { get; set; }
        public decimal CoveragePersonLimit { get; set; }
        public decimal CoveragePropertyLimit { get; set; }
        public decimal CoverageVehiclePercentage { get; set; }
        public decimal CoverageTheft { get; set; }
        public decimal DeductibleAmount { get; set; }
        public string InsuranceDescription { get; set; } = string.Empty;

        // Incident Images
        public List<string> IncidentImages { get; set; } = new();

        public DateTimeOffset CreatedAt { get; set; }
    }
}
