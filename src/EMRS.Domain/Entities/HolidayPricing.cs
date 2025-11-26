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
        public DateOnly? HolidayDate { get; set; }
        public decimal PriceMultiplier { get; set; }

        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
