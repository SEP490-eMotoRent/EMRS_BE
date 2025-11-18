using EMRS.Application.DTOs.MediaDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.TicketDTOs
{
    public class TicketDetailResponse
    {
        public Guid Id { get; set; }
        public string TicketType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public Guid BookingId { get; set; }
        public Guid? StaffId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<MediaResponse>? Attachments { get; set; } 
    }
}
