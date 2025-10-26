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
        public async Task<IActionResult> GetAllBooking()
        {

            var result = await _bookingService.GetAllBookings();
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
