using EMRS.Application.DTOs.MediaDTOs;
using EMRS.Application.DTOs.VehicleDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.VehicleModelDTOs;

public class VehicleModelDetailListResponse
{
    public Guid VehicleModelId { get; set; }
    public string ModelName { get; set; }
    public string Category { get; set; }
    public decimal BatteryCapacityKwh { get; set; }
    public decimal MaxRangeKm { get; set; }
    public decimal RentalPrice { get; set; }
    public string ImageUrl { get; set; }
    public decimal OriginalRentalPrice { get; set; }
    public List<ColorResponse> AvailableColors { get; set; }
    public int CountTotal { get; set; }
    public int CountAvailable { get; set; }
    public List<VehiclDetailListForVehicleModelListResponse>? Vehicles { get; set; }
    public List<MediaResponse>? mediaResponses { get; set; }
}
public class VehiclDetailListForVehicleModelListResponse 
{
    public Guid Id { get; set; }
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
    public List<MediaResponse> mediaResponses { get; set; }
}