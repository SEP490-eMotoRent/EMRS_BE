using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.VehicleDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.VehicleTransferDTOs
{
    public class VehicleTransferOrderDetailResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public DateTimeOffset? ReceivedDate { get; set; }
        public string Notes { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        // Nested objects
        public VehicleResponse Vehicle { get; set; }
        public BranchResponse FromBranch { get; set; }
        public BranchResponse ToBranch { get; set; }
        public List<VehicleTransferRequestResponse> VehicleTransferRequests { get; set; }
    }
}
