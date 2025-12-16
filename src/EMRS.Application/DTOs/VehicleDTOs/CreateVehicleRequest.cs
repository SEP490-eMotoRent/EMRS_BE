using EMRS.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.VehicleDTOs;

public class CreateVehicleRequest
{
    public string LicensePlate { get; set; }
    public string Color { get; set; }
    public string? GpsDeviceIdent { get; set; }

    public int? FlespiDeviceId { get; set; }
    public DateTime? DateManufacturing { get; set; }
    public decimal CurrentOdometerKm { get; set; }
    public decimal BatteryHealthPercentage { get; set; }
    public VehicleStatusEnum Status { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string Description { get; set; }
    

    public Guid VehicleModelId { get; set; }

    public Guid BranchId { get; set; }

    public List<IFormFile?>? ImageFiles { get; set; }
}
