using EMRS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.VehicleDTOs;

public class VehicleUpdateRequest
{
    public Guid VehicleId { get; set; }
    public string LicensePlate { get; set; }
    public string Color { get; set; }
    public DateTime? YearOfManufacture { get; set; }
    public decimal CurrentOdometerKm { get; set; }
    public decimal BatteryHealthPercentage { get; set; }
    public VehicleStatusEnum Status { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDue { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string Description { get; set; }
}
