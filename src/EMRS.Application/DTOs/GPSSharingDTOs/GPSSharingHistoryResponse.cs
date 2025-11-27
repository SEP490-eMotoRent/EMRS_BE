using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.GPSSharingDTOs
{
    public class GPSSharingHistoryResponse
    {
        public Guid SessionId { get; set; }
        public string InvitationCode { get; set; }
        public string Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? AcceptedAt { get; set; }
        public DateTimeOffset? SessionExpiresAt { get; set; }

        // Owner info
        public Guid OwnerRenterId { get; set; }
        public string OwnerRenterName { get; set; }
        public string OwnerVehicleLicensePlate { get; set; }

        // Guest Info
        public Guid? GuestRenterId { get; set; }
        public string? GuestRenterName { get; set; }
        public string? GuestVehicleLicensePlate { get; set; }
    }
}
