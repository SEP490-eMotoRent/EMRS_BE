using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.VehicleModelDTOs;

public class VehicleModelCreateRequest
{
    public string ModelName { get; set; }
    public string Category { get; set; }
    public decimal BatteryCapacityKwh { get; set; }
    public decimal MaxRangeKm { get; set; }
    public decimal MaxSpeedKmh { get; set; }
    public string Description { get; set; }

    public Guid RentalPricingId { get; set; }
}
