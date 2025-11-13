using EMRS.Application.DTOs.StaffDTOs;
using EMRS.Application.DTOs.VehicleModelDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.VehicleTransferDTOs
{
    public class VehicleTransferRequestDetailResponse
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public decimal QuantityRequested { get; set; }
        public DateTime RequestedAt { get; set; }
        public string Status { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        // Nested objects
        public VehicleModelResponse VehicleModel { get; set; }
        public StaffResponse Staff { get; set; }
        public VehicleTransferOrderResponse? VehicleTransferOrder { get; set; }
    }
}
