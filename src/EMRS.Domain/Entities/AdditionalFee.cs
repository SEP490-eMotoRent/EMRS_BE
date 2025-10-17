using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class AdditionalFee : BaseEntity
    {
        public string FeeType { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid BookingId { get; set; }

        //relationship
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; } = null!;
    }
}