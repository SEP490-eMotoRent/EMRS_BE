using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.HolidayPricingDTOs
{
    public class HolidayPricingCreateRequest
    {
        public string HolidayName { get; set; }
        public DateTime? HolidayDate { get; set; }
        public decimal PriceMultiplier { get; set; }
        public string Description { get; set; } 
        public bool IsActive { get; set; }
    }
}
