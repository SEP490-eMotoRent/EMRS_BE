using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.AdditionalFeeDTOs
{
    public class AdditionalFeeResponse
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public string FeeType { get; set; } // "LATE_RETURN", "CLEANING", etc.
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
