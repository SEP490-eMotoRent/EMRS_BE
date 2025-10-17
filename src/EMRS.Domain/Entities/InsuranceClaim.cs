using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class InsuranceClaim : BaseEntity
    {
        public DateTime? IncidentDate { get; set; }
        public string IncidentLocation { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; }
        public string Status { get; set; }
        public decimal VehicleDamageCost { get; set; }
        public decimal PersonInjuryCost { get; set; }
        public decimal ThirdPartyCost { get; set; }
        public decimal TotalCost { get; set; }
        public decimal InsuranceCoverageAmount { get; set; }
        public decimal RenterLiabilityAmount { get; set; }
        public DateTime? ReportedAt { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
        public string? InsuranceClaimPdfUrl { get; set; }
        public string Notes { get; set; } = string.Empty;
        public Guid RenterId { get; set; }

        public Guid  BookingId { get; set; }

        //relationship
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; } = null!;
        [ForeignKey(nameof(RenterId))]
        public Renter Renter { get; set; } = null!;

    }
}
