using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class ChargingRecord:BaseEntity
    {
        public DateTime? ChargingDate { get; set; }
        public decimal StartBatteryPercentage { get; set; }
        public decimal EndBatteryPercentage { get; set; }
        public decimal KwhCharged { get; set; }
        public decimal RatePerKwh { get; set; }
        public decimal Fee { get; set; }
        public string Notes { get; set; }= string.Empty;
        public Guid BookingId { get; set; }

        public Guid BranchId { get; set; }

        public Guid StaffId { get; set; }
        //relationship
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; } = null!;
        [ForeignKey(nameof(BranchId))]
        public Branch Branch { get; set; } = null!;
        [ForeignKey(nameof(StaffId))]
        public Staff Staff { get; set; } = null!;

    }
}
