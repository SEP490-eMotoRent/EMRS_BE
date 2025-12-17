using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.InsurancePackageDTOs;
using EMRS.Application.DTOs.MediaDTOs;
using EMRS.Application.DTOs.VehicleDTOs;
using EMRS.Application.DTOs.VehicleModelDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.BookingDTOs;

public class BookingListForRenterResponse
{
    public Guid Id { get; set; }
    public DateTime? StartDatetime { get; set; }
    public DateTime? EndDatetime { get; set; }
    public DateTime? ActualReturnDatetime { get; set; }
    public decimal BaseRentalFee { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal RentalDays { get; set; }
    public decimal RentalHours { get; set; }
   
    public decimal LateReturnFee { get; set; }
    public decimal AverageRentalPrice { get; set; }
    public decimal TotalRentalFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string BookingStatus { get; set; }

    public Guid VehicleModelId { get; set; }
    public Guid RenterId { get; set; }
    public VehicleForBookingRenter? vehicle { get; set; }

    public VehicleModelResponse vehicleModel { get; set; }
    public RenterDetailResponse renter { get; set; }
    public MediaResponse vehicleModelmediaResponse { get; set; }
    public InsurancePackageResponse insurancePackage { get; set; }
    public BranchResponse HandoverBranch { get; set; }
    public BranchResponse ReturnBranch { get; set; }
}

public class VehicleForBookingRenter
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
}
