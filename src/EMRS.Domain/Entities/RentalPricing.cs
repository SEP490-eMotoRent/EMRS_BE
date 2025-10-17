using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class RentalPricing:BaseEntity
    {
        public decimal RentalPrice { get; set; }

        public decimal ExcessKmPrice { get; set; }

        //relationship
        public ICollection<VehicleModel> VehicleModels { get; set; } = new List<VehicleModel>();

    }
}
