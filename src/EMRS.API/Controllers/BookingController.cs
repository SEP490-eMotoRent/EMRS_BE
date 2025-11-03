using EMRS.Application.Abstractions;
using EMRS.Application.DTOs.BookingDTOs;
using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {

        private readonly IBookingService _bookingService;
        private readonly IVNPayService _vNPayService;
        public BookingController(IVNPayService vNPayService,IBookingService bookingService)
        {
            _bookingService = bookingService;
            _vNPayService = vNPayService;
        }
        [Authorize(Roles ="RENTER")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] BookingCreateRequest request)
        {

            var result = await _bookingService.CreateBooking(request);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [Authorize(Roles = "RENTER")]
        [HttpPost("vnpay")]
        public async Task<IActionResult> CreateVnpayBooking([FromBody] BookingCreateRequest request)
        {

            var result = await _bookingService.CreateBookingWithoutWallet(request);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpGet("callback/vnpay")]
        public async Task<IActionResult> Vnpaycallback( )
        {

            var result = await _vNPayService.ProcessResponse(request);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [Authorize(Roles = "RENTER")]
        [HttpGet("renter/get")]
        public async Task<IActionResult> GetAll()
        {

            var result = await _bookingService.GetAllBookingsByRenterIdAsync();
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [Authorize(Roles = "STAFF")]
        [HttpGet("")]
        public async Task<IActionResult> GetAllBooking(  Guid? VehicleModelId, Guid? RenterId ,string? BookingStatus,int PageNum, int PageSize )
        {
            var request = new BookingSearchRequest
            {
                VehicleModelId = VehicleModelId,
                RenterId = RenterId,
                BookingStatus = BookingStatus
            };
            var result = await _bookingService.GetAllBookings(request,PageNum,PageSize);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetBookingDetail(Guid bookingId)
        {
        
            var result = await _bookingService.GetBookingDetailAsync(bookingId);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [Authorize(Roles = "STAFF")]
        [HttpPut("vehicle/assign/{bookingId}/{vehicleId}")]
        public async Task<IActionResult> AssignVehicleForBooking(Guid bookingId,Guid vehicleId)
        {
           
            var result = await _bookingService.AssignVehicleForBookingIfBooked(bookingId,vehicleId);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [Authorize(Roles = "STAFF")]
        [HttpPut("vehicle/{bookingId}/{vehicleId}")]
        public async Task<IActionResult> EditVehicleModelForBooking(Guid bookingId, Guid vehicleId)
        {

            var result = await _bookingService.UpdateVehicleForBooking(bookingId, vehicleId);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
    }
}
