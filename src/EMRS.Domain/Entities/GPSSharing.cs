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
        //Relationship


        [ForeignKey(nameof(OwnerRenterId))]
        [InverseProperty(nameof(Renter.OwnerGPSSharings))]
        public Renter? OwnerRenter { get; set; }

        // Guest Renter
        [ForeignKey(nameof(GuestRenterId))]
        [InverseProperty(nameof(Renter.GuestGPSSharings))]
        public Renter? GuestRenter { get; set; }

        [ForeignKey(nameof(OwnerVehicleId))]
        [InverseProperty(nameof(Vehicle.OwnerGPSSharings))]
        public Vehicle? OwnerVehicle { get; set; }

        // Guest Vehicle
        [ForeignKey(nameof(GuestVehicleId))]
        [InverseProperty(nameof(Vehicle.GuestGPSSharings))]
        public Vehicle? GuestVehicle { get; set; }

    }

}
