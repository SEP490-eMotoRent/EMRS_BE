using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.HolidayPricingDTOs
{
    public class HolidayPricingResponse
    {
        public Guid Id { get; set; }
        public string HolidayName { get; set; } = string.Empty;
        public DateTime? HolidayDate { get; set; }
        public decimal PriceMultiplier { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
