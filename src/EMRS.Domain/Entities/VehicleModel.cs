using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class VehicleModel: BaseEntity
    {
        public string ModelName { get; set; }
        public string Category { get; set; }
        public decimal BatteryCapacityKwh { get; set; }
        public decimal MaxRangeKm { get; set; }
        public decimal MaxSpeedKmh { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public Guid RentalPricingId { get; set; }
        //relationship
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        [ForeignKey(nameof(RentalPricingId))]
        public RentalPricing RentalPricing { get; set; } = null!;

    }
}
