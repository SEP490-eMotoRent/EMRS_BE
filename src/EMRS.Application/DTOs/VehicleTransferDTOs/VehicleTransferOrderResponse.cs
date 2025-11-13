using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.VehicleTransferDTOs
{
    public class VehicleTransferOrderResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string Notes { get; set; }
        public Guid VehicleId { get; set; }
        public string VehicleLicensePlate { get; set; }
        public Guid FromBranchId { get; set; }
        public string FromBranchName { get; set; }
        public Guid ToBranchId { get; set; }
        public string ToBranchName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
