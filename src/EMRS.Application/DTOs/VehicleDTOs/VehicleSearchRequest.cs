using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.VehicleDTOs;

public class VehicleSearchRequest
{
    public string? LicensePlate { get; set; }
    public string? Color { get; set; }

    public decimal? CurrentOdometerKm { get; set; }
    public decimal? BatteryHealthPercentage { get; set; }
    public string? Status { get; set; }



/*    public Guid? BranchId { get; set; }
    public Guid? VehicleModelId { get; set; }*/
}
