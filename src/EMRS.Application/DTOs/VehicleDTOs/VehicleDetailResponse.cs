using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.RentalPricingDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.VehicleDTOs;

public class VehicleDetailResponse
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
    public VehicleModelReponseWithRentalPricing vehicleModel { get; set; }
    public BranchResponse branch { get; set; }
}

public class VehicleModelReponseWithRentalPricing
{
    public Guid Id { get; set; }
    public string ModelName { get; set; }
    public string Category { get; set; }
    public decimal BatteryCapacityKwh { get; set; }
    public decimal MaxRangeKm { get; set; }
    public decimal MaxSpeedKmh { get; set; }
    public string Description { get; set; }

    public RentalPricingResponse RentalPricingResponse { get; set; }
}

