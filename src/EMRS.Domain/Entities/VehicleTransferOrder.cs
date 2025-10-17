using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class VehicleTransferOrder : BaseEntity
    {
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string Notes { get; set; }

        public Guid FromBranchId { get; set; }
        public Guid ToBranchId { get; set; }

        //relationship
        [ForeignKey(nameof(FromBranchId))]
        [InverseProperty(nameof(Branch.SentTransferOrders))]
        public Branch FromBranch { get; set; } = null!;

        [ForeignKey(nameof(ToBranchId))]
        [InverseProperty(nameof(Branch.ReceivedTransferOrders))]
        public Branch ToBranch { get; set; } = null!;

        public ICollection<VehicleTransferRequest> VehicleTransferRequests { get; set; } = new List<VehicleTransferRequest>();
        
    }
}
