
using EMRS.Application.DTOs.RentalPricingDTOs;
using EMRS.Application.DTOs.VehicleModelDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.VehicleDTOs;

public class VehicleListResponse
{
    public Guid Id { get; set; }
    public string Color { get; set; }
    public decimal CurrentOdometerKm { get; set; }
    public decimal BatteryHealthPercentage { get; set; }
    public string Status { get; set; }
    public string LicensePlate { get; set; }

    public DateTime? NextMaintenanceDue { get; set; }
    public List<string>? FileUrl { get; set; }
    public RentalPricingResponse rentalPricing {  get; set; }
    public VehicleModelResponse vehicleModel {  get; set; }

}

