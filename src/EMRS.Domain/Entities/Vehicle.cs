using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class Vehicle:BaseEntity
    {
        public string LicensePlate { get; set; }
        public string Color { get; set; }
        public DateTime? YearOfManufacture { get; set; }
        public decimal CurrentOdometerKm { get; set; }
        public decimal BatteryHealthPercentage { get; set; }
        public string Status { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDue { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string Description { get; set; }
        public Guid BranchId { get; set; }
        public Guid VehicleModelId { get; set; }


        //relationship
        [ForeignKey(nameof(BranchId))]
        public Branch Branch { get; set; } = null!;
        [ForeignKey(nameof(VehicleModelId))]
        public VehicleModel VehicleModel { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        public ICollection<MaintenanceSchedule> MaintenanceSchedules { get; set; } = new List<MaintenanceSchedule>();
        public ICollection<RepairRequest> RepairRequests { get; set; }= new List<RepairRequest>();
    }
}
