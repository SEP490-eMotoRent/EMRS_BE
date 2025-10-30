using EMRS.Application.DTOs.RentalContractDTOs;
using EMRS.Application.DTOs.RentalPricingDTOs;
using EMRS.Application.DTOs.VehicleModelDTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.BookingDTOs;

public class BookingDetailResponse
{
    public Guid Id { get; set; }
    public DateTime? StartDatetime { get; set; }
    public DateTime? EndDatetime { get; set; }
    public DateTime? ActualReturnDatetime { get; set; }
    public decimal BaseRentalFee { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal RentalDays { get; set; }
    public decimal RentalHours { get; set; }
    public decimal RentingRate { get; set; }
    public decimal LateReturnFee { get; set; }
    public decimal AverageRentalPrice { get; set; }
    public decimal TotalRentalFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string BookingStatus { get; set; }

   
    public VehicleBookingDetailResponse vehicle { get; set; }
    public VehicleModelResponse vehicleModel {  get; set; }
    public RenterDetailResponse renter { get; set; }
    public RentalContractResponse rentalContract { get; set; }  
    
}
public class VehicleBookingDetailResponse
{
    public Guid Id { get; set; }
    public string Color { get; set; }
    public decimal CurrentOdometerKm { get; set; }
    public decimal BatteryHealthPercentage { get; set; }
    public string Status { get; set; }
    public string LicensePlate { get; set; }

    public DateTime? NextMaintenanceDue { get; set; }
    public List<string>? FileUrl { get; set; }
    public RentalPricingResponse rentalPricing { get; set; }
}
public class RenterDetailResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string phone { get; set; }
    public string Address { get; set; }
    public string? DateOfBirth { get; set; }
  

    public BookingDetailAccountResponse account { get; set; }
}
public class BookingDetailAccountResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } 


    public string Role { get; set; }

    public string? Fullname { get; set; }


}

