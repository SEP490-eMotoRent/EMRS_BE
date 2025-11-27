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

        public Guid OwnerRenterId { get; set; }
        public Guid OwnerVehicleId { get; set; }

        public Guid? GuestRenterId { get; set; }
        public Guid? GuestVehicleId { get; set; }

        [ForeignKey(nameof(OwnerRenterId))]
        public Renter OwnerUser { get; set; } = null!;

        [ForeignKey(nameof(OwnerVehicleId))]
        public Vehicle OwnerVehicle { get; set; } = null!;

        [ForeignKey(nameof(GuestRenterId))]
        public Renter? GuestUser { get; set; }

        [ForeignKey(nameof(GuestVehicleId))]
        public Vehicle? GuestVehicle { get; set; }
    }

}
