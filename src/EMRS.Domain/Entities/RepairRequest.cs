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
        public string? Priority { get; set; }
        public string Status { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
        //jsonb
        [Column(TypeName = "jsonb")]
        public string? Checklist { get; set; } 
        public Guid VehicleId { get; set; }

        public Guid? TechnicianId { get; set; }

        //relationship
        [ForeignKey(nameof(TechnicianId))]
        public Staff Staff { get; set; } = null!;
        [ForeignKey(nameof(VehicleId))]
        public Vehicle Vehicle { get; set; } = null!;

    }
}
