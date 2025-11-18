using EMRS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.TicketDTOs
{
    public  class TicketUpdateRequest
    {
        public Guid Id { get; set; }
        public TicketStatusEnum Status { get; set; }
        public Guid? StaffId { get; set; }
    }
}
