using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.VehicleTransferDTOs
{
    public class VehicleTransferRequestCreateRequest
    {
        public Guid VehicleModelId { get; set; }
        public decimal QuantityRequested { get; set; }
        public string Description { get; set; }
    }
}
