using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.GPSSharingDTOs
{
    public class GPSSharingInviteResponse
    {
        public Guid SessionId { get; set; }
        public string InvitationCode { get; set; }
        public DateTimeOffset InvitationExpiresAt { get; set; } // 30 phút
        public string OwnerVehicleLicensePlate { get; set; }
    }
}
