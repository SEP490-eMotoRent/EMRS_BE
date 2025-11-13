using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.VehicleTransferDTOs
{
    public class VehicleTransferRequestResponse
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public decimal QuantityRequested { get; set; }
        public DateTime RequestedAt { get; set; }
        public string Status { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid VehicleModelId { get; set; }
        public string VehicleModelName { get; set; }
        public Guid StaffId { get; set; }
        public string StaffName { get; set; }
        public string BranchName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
