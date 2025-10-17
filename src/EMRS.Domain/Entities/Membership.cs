using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class Membership:BaseEntity
    {

        public string TierName { get; set; }
        public decimal MinBookings { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal FreeChargingPerMonth { get; set; }
        public string Description { get; set; }

        //relationship
        public ICollection<Renter> Renters { get; set; } = new List<Renter>();
    }
}
