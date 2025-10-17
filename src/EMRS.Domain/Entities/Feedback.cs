using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class Feedback : BaseEntity
    {
        public decimal Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime? FeedbackDate { get; set; }
      
        public DateTime? ResponseDate { get; set; }
        public Guid RenterId { get; set; }

        public Guid BookingId { get; set; }

        //relationship
        [ForeignKey(nameof(RenterId))]
        public Renter Renter { get; set; } = null!;

        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; } = null!;
    }
} 