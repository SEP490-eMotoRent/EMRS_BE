using EMRS.Application.DTOs.VehicleDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.GPSSharingDTOs
{
    public class GPSSharingActiveResponse
    {
        public Guid SessionId { get; set; }
        public DateTimeOffset SessionExpiresAt { get; set; } 
        public ParticipantTrackingInfo OwnerInfo { get; set; }
        public ParticipantTrackingInfo GuestInfo { get; set; }
    }

    public class ParticipantTrackingInfo
    {
        public Guid RenterId { get; set; }
        public string RenterName { get; set; }
        public VehicleTrackingResponse Vehicle { get; set; }
        public string? AvatarRenter {  get; set; }
    }
}
