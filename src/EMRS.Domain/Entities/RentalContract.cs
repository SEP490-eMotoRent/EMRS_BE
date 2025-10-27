using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class RentalContract : BaseEntity
    {
   
        public string OtpCode { get; set; }
        public DateTime? ExpireAt { get; set; }
        public string ContractStatus { get; set; }
        public string ContractPdfUrl { get; set; }

        public Guid BookingId { get; set; }
        //relationship
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; } = null!;

    }
}
