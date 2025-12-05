using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.GPSSharingDTOs
{
    public class GPSSharingSessionResponse
    {
        public Guid SessionId { get; set; }
        public string InvitationCode { get; set; }
        public string Status { get; set; }
        public DateTimeOffset? InvitationExpiresAt { get; set; }
        public DateTimeOffset? SessionExpiresAt { get; set; }
        public DateTimeOffset? AcceptedAt { get; set; }
        public ParticipantTrackingInfo? OwnerInfo { get; set; }
        public ParticipantTrackingInfo? GuestInfo { get; set; }
        public string? AvatarOwner { get; set; }
        public string? AvatarGuest { get; set; }
    }
}
