using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class MaintenanceRecord:BaseEntity

    {
        public DateTime? MaintenanceDate { get; set; }
        public string IssuesFound { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }

        public Guid StaffId { get; set; }

        public Guid MaintenanceScheduleId { get; set; }
        //relationship
        [ForeignKey(nameof(StaffId))]
        public Staff Staff { get; set; } = null!;
        [ForeignKey(nameof(MaintenanceScheduleId))]
        public MaintenanceSchedule MaintenanceSchedule { get; set; } = null!;
    }
}
