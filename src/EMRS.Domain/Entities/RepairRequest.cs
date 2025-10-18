using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class RepairRequest:BaseEntity
    {
        public string IssueDescription { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public Guid VehicleId { get; set; }

        public Guid StaffId { get; set; }

        //relationship
        [ForeignKey(nameof(StaffId))]
        public Staff Staff { get; set; } = null!;
        [ForeignKey(nameof(VehicleId))]
        public Vehicle Vehicle { get; set; } = null!;

    }
}
