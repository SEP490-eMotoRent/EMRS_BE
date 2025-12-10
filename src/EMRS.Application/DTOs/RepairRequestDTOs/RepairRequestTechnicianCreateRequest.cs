using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RepairRequestDTOs
{
    public class RepairRequestTechnicianCreateRequest
    {
        public string IssueDescription { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }

        public Guid VehicleId { get; set; }

        public Guid? TechnicianId { get; set; }
    }
}
