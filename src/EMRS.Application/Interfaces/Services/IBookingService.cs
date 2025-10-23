using EMRS.Application.Common;
using EMRS.Application.DTOs.BookingDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services;

public interface IBookingService
{
    Task<ResultResponse<BookingResponse>> CreateBooking(BookingCreateRequest bookingCreateRequest);
    Task<ResultResponse<List<BookingResponse>>> GetAllBookingsByRenterIdAsync();
}
