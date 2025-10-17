using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class MaintenanceSchedule:BaseEntity
    {

        public string ScheduleTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FrequencyType { get; set; }
        public decimal FrequencyValueKm { get; set; }
        public decimal FrequencyValueDays { get; set; }
        public string Checklist { get; set; }

        public DateTime? CreatedAt { get; set; }

        public Guid VehicleId { get; set; }
        public Guid  StaffId { get; set; }
        //relationship
        [ForeignKey(nameof(StaffId))]
        public Staff Staff { get; set; } = null!;
        [ForeignKey(nameof(VehicleId))]
        public Vehicle Vehicle { get; set; } = null!;
        public MaintenanceRecord? MaintenanceRecord { get; set; }
    }
}
