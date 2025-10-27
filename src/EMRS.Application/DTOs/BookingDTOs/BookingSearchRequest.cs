using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.BookingDTOs;

public class BookingSearchRequest
{
    public Guid? VehicleModelId { get; set; }
    public Guid? RenterId { get; set; }
    public string? BookingStatus { get; set; }
}
