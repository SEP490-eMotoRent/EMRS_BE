using EMRS.Application.DTOs.RentalPricingDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalReceiptDTOs;

public class RentalReceiptListResponse
{



    public Guid Id { get; set; }



    public string? Notes { get; set; }
    public DateTime? RenterConfirmedAt { get; set; }
    public decimal StartOdometerKm { get; set; }
    public decimal StartBatteryPercentage { get; set; }
    public Guid BookingId { get; set; }
    public Guid StaffId { get; set; }
    public decimal EndBatteryPercentage { get; set; }
    public RentalDetailVehicleResponse? Vehicle { get; set; }
    public decimal EndOdometerKm { get; set; }

    public List<string>? HandOverVehicleImageFiles { get; set; } = new List<string>();
    public List<string>? ReturnVehicleImageFiles { get; set; } = new List<string>();

    public List<string>? CheckListHandoverFile { get; set; } = new List<string>();
    public List<string>? CheckListReturnFile { get; set; } = new List<string>();
}
public class RentalDetailVehicleResponse
{

    public Guid Id { get; set; }
    public string LicensePlate { get; set; }
    public string Color { get; set; }
    public DateTime? DateManufacturing { get; set; }
    public decimal CurrentOdometerKm { get; set; }
    public decimal BatteryHealthPercentage { get; set; }
    public string Status { get; set; }

    public DateTime? PurchaseDate { get; set; }
    public string Description { get; set; }
    public List<string>? VehicleImageFiles { get; set; } = new List<string>();
    public RentalPricingResponse? rentalPricing { get; set; } = null;


}
