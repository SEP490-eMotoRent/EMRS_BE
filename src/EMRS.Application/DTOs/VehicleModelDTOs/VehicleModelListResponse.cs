using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.VehicleModelDTOs;

public class VehicleModelListResponse
{
    public Guid VehicleModelId { get; set; }
    public string ModelName { get; set; }
    public string Category { get; set; }
    public decimal BatteryCapacityKwh { get; set; }
    public decimal MaxRangeKm { get; set; }
    public decimal RentalPrice { get; set; }
    public string ImageUrl { get; set; }
    public List<ColorResponse> AvailableColors { get; set; }
}
public class ColorResponse
{
    public string ColorName { get; set; }
}
