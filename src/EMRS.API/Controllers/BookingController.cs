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

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
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
        [Authorize(Roles = "STAFF")]
        [HttpPut("{bookingId}/{vehicleId}")]
        public async Task<IActionResult> AssignVehicleForBooking(Guid bookingId,Guid vehicleId)
        {
           
            var result = await _bookingService.AssignVehicleForBooking(bookingId,vehicleId);
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
