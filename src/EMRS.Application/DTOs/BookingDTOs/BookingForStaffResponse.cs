using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.BookingDTOs;

public class BookingForStaffResponse
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

    public RenterBookingResponse Renter { get; set; }

    public VehicleBookingResponse Vehicle { get; set; }
    public VehilceModelBookingResponse VehicleModel { get; set; }


}
public class RenterBookingResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string phone { get; set; }
    public string Address { get; set; }
    public AccountBookingResponse Account { get; set; }
}
public class AccountBookingResponse
{
    public Guid Id { get; set; }

    public string Username { get; set; }

    public string Role { get; set; }

    public string? Fullname { get; set; }

}
public class VehicleBookingResponse
{
    public Guid Id { get; set; }
    public string Color { get; set; }
    public decimal CurrentOdometerKm { get; set; }
    public decimal BatteryHealthPercentage { get; set; }
    public string Status { get; set; }
    public string LicensePlate { get; set; }

    public DateTime? NextMaintenanceDue { get; set; }
    public List<string>? FileUrl { get; set; }
    public decimal RentalPricing { get; set; }
    public VehilceModelBookingResponse VehicleModel { get; set; }

}
public class VehilceModelBookingResponse
{
    public Guid Id { get; set; }
    public string ModelName { get; set; }
    public string Category { get; set; }
    public decimal BatteryCapacityKwh { get; set; }
    public decimal MaxRangeKm { get; set; }
    public decimal MaxSpeedKmh { get; set; }
}