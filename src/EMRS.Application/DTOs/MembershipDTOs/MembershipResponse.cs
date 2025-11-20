using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.MembershipDTOs
{
    public class MembershipResponse
    {
        public Guid Id { get; set; }
        public string TierName { get; set; }
        public decimal MinBookings { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
