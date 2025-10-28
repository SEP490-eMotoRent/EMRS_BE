using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class VehicleTransferRequest : BaseEntity
    {
        public string Description { get; set; }
        public decimal QuantityRequested { get; set; }
        public DateTime? RequestedAt { get; set; }
        public string Status { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public Guid VehicleModelId { get; set; }
        public Guid? VehicleTransferOrderId { get; set; }
        public Guid StaffId { get; set; }
        //relationship  
        public Staff Staff { get; set; } = null!;   

        public VehicleTransferOrder? VehicleTransferOrder { get; set; }
        public VehicleModel VehicleModel { get; set; } = null!;
    }
}
