using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.BookingDTOs;

public class BookingCreateRequest
{
    public DateTime? StartDatetime { get; set; }
    public DateTime? EndDatetime { get; set; }

    public Guid HandoverBranchId {  get; set; }
    public decimal BaseRentalFee { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal RentalDays { get; set; }
    public decimal RentalHours { get; set; }
    public decimal RentingRate { get; set; }
    public Guid VehicleModelId { get; set; }
    public decimal AverageRentalPrice { get; set; }


    public Guid? InsurancePackageId { get; set; }
    public decimal TotalRentalFee { get; set; }


}
