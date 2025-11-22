using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RepairRequestDTOs
{
    public  class RepairRequestCreateRequest
    {
        public string IssueDescription { get; set; }

        public Guid VehicleId { get; set; }

    }
}
