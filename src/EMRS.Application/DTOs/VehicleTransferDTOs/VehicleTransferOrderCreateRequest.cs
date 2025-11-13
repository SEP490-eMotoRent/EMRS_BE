using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.VehicleTransferDTOs
{
    public class VehicleTransferOrderCreateRequest
    {
        public Guid VehicleId { get; set; }
        public Guid FromBranchId { get; set; }
        public Guid ToBranchId { get; set; }
        public string Notes { get; set; }
    }
}
