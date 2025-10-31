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

        // Vehicle Model
        public string ModelName { get; set; } = null!;

        // Vehicle
        public string LicensePlate { get; set; } = null!;

        // Insurance Package
        public string PackageName { get; set; } = null!;
        public decimal PackageFee { get; set; }
        public decimal CoveragePersonLimit { get; set; }
        public decimal CoveragePropertyLimit { get; set; }
        public decimal CoverageVehiclePercentage { get; set; }
        public decimal CoverageTheft { get; set; }
        public decimal DeductibleAmount { get; set; }

        // Payment Status (nullable - chỉ có khi Status = Completed)
        public decimal? VehicleDamageCost { get; set; }
        public decimal? PersonInjuryCost { get; set; }
        public decimal? ThirdPartyCost { get; set; }
        public decimal? TotalCost { get; set; }
        public decimal? InsuranceCoverageAmount { get; set; }
        public decimal? RenterLiabilityAmount { get; set; }

        public Guid BookingId { get; set; }
        public Guid RenterId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
