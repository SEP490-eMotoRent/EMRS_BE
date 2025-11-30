using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class GPSSharing : BaseEntity
    {

        public string InvitationCode { get; set; } 
        public string Status { get; set; } 

        public DateTimeOffset ExpiresAt { get; set; } 
        public DateTimeOffset? AcceptedAt { get; set; }
        public DateTimeOffset? SessionExpiresAt { get; set; }
        public Guid OwnerBookingId { get; set; }
        public Guid? GuestBookingId { get; set; } 
        //Relationship


     

        [ForeignKey(nameof(OwnerBookingId))]
        [InverseProperty(nameof(Booking.OwnerGPSSharings))]
        public Booking OwnerBooking { get; set; } = null!;

        [ForeignKey(nameof(GuestBookingId))]
        [InverseProperty(nameof(Booking.GuestGPSSharings))]
        public Booking? GuestBooking { get; set; }

    }

}
