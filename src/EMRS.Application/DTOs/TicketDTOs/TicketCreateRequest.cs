using EMRS.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.TicketDTOs
{
    public class TicketCreateRequest
    {
        public TicketTypeEnum TicketType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid BookingId { get; set; }

        public List<IFormFile>? Attachments { get; set; }
    }
}
