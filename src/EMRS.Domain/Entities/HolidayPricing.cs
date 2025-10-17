using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class HolidayPricing : BaseEntity
    {
        public string HolidayName { get; set; } = string.Empty;
        public DateTime? HolidayDate { get; set; }
        public decimal PriceMultiplier { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
