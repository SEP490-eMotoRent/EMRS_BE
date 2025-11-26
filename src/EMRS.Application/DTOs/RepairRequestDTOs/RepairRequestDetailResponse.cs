using EMRS.Application.DTOs.BranchDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RepairRequestDTOs
{
    public class RepairRequestDetailResponse
    {
        public Guid Id { get; set; }
        public string IssueDescription { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
        public BranchResponse? branch { get; set; }  
        public Guid VehicleId
        { get; set; }

        public Guid? TechnicianId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
    