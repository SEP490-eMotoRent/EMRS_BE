using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class InsurancePackage : BaseEntity
    {
        public string PackageName { get; set; } = string.Empty;
        public decimal PackageFee { get; set; }
        public decimal CoveragePersonLimit { get; set; }
        public decimal CoveragePropertyLimit { get; set; }
        public decimal CoverageVehiclePercentage { get; set; }
        public decimal CoverageTheft { get; set; }
        public decimal DeductibleAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        //relationship
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();   
    }
}
