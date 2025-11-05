using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class RentalReceipt : BaseEntity
    {
     
        public string? Notes { get; set; }
        public DateTime? RenterConfirmedAt { get; set; }
        public decimal StartOdometerKm { get; set; }
        public decimal EndOdometerKm { get; set; }
        public decimal StartBatteryPercentage { get; set; }
        public decimal EndBatteryPercentage { get; set; }
        public Guid BookingId { get; set; }
        public Guid StaffId { get; set; }
        public Guid VehicleId { get; set; }
        public Guid VehicleModelId { get; set; }

        //relationship
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; } = null!;
        [ForeignKey(nameof(StaffId))]
        public Staff Staff { get; set; } = null!;
        [ForeignKey(nameof(VehicleId))]
        public Vehicle Vehicle { get; set; } = null!;
        [ForeignKey(nameof(VehicleModelId))]
        public VehicleModel VehicleModel { get; set; } = null!;

    }
}
