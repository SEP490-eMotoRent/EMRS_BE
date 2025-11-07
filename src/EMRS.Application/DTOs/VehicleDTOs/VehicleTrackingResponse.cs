using EMRS.Application.DTOs.RentalPricingDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.VehicleDTOs;

public class VehicleTrackingResponse
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

    public RentalPricingResponse? rentalPricing { get; set; } = null;
    public ProtrackResponse protrackResponse { get; set; }
}
public class ProtrackResponse
{
    public string access_token { get; set; }
    public long expires_in { get; set; }
}

