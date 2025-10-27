using EMRS.Application.DTOs.VehicleDTOs;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.BookingDTOs;

public class BookingResponse
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

    public Guid VehicleModelId { get; set; }
    public Guid RenterId { get; set; }
    public Guid? VehicleId { get; set; }


}
