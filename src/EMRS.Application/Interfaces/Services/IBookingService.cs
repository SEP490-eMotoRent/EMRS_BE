using EMRS.Application.Abstractions.Models.VNPay;
using EMRS.Application.Common;
using EMRS.Application.DTOs.BookingDTOs;
using EMRS.Application.DTOs.RentalReceiptDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services;

public interface IBookingService
{
    Task<ResultResponse<PaginationResult<List<BookingResponse>>>> GetBookingByHandoverIdAsync(Guid branchId, int PageNum, int PageSize, bool orderByDescending);
    Task<ResultResponse<BookingResponse>> CancelBookingByStaffAsync(Guid bookingId);
    Task<ResultResponse<BookingResponse>> CancelBookingByCustomerAsync(Guid bookingId);
    Task<ResultResponse<bool>> ProcessCallBack(VNPayResponseData vNPayResponseData);
    Task<ResultResponse<BookingWithoutWalletResponse>> CreateBookingWithoutWallet(BookingCreateRequest bookingCreateRequest);
    Task<ResultResponse<BookingResponse>> CreateBooking(BookingCreateRequest bookingCreateRequest);
    Task<ResultResponse<List<BookingListForRenterResponse>>> GetAllBookingsByRenterIdAsync();
    Task<ResultResponse<BookingResponse>> AssignVehicleForBookingIfBooked(Guid bookingId, Guid vehicleId);
    Task<ResultResponse<BookingDetailResponse>> GetBookingDetailAsync(Guid bookingId);
    Task<ResultResponse<BookingResponse>> UpdateVehicleForBooking(Guid bookingId, Guid vehicleId);
    Task<ResultResponse<PaginationResult<List<BookingForStaffResponse>>>> GetAllBookings(BookingSearchRequest bookingSearchRequest, int PageNum, int PageSize);
}
