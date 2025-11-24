using EMRS.Application.DTOs.TicketDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] TicketCreateRequest request)
        {
            var result = await _ticketService.CreateTicketAsync(request);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }


        [Authorize(Roles = "MANAGER")]

        [HttpGet("")]
        public async Task<IActionResult> GetAll(
            int pageSize,
            int pageNum,
            bool orderByDescending)
        {
            var result = await _ticketService.GetAllTicketAsync(pageSize, pageNum, orderByDescending);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }


       
        [HttpGet("staff/{staffId}")]
        public async Task<IActionResult> GetAllByStaffId(
            Guid staffId,
            int pageSize,
            int pageNum,
            bool orderByDescending)
        {
            var result = await _ticketService.GetAllTicketByStaffIdAsync(staffId, pageSize, pageNum, orderByDescending);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetAllByBookingId(
           Guid bookingId,
           int pageSize,
           int pageNum,
           bool orderByDescending)
        {
            var result = await _ticketService.GetAllTicketByBookingIdAsync(bookingId, pageSize, pageNum, orderByDescending);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }


        // ===============================
        // GET BY ID (Ticket detail)
        // ===============================
        [HttpGet("{ticketId}")]
        public async Task<IActionResult> GetTicketDetail(Guid ticketId)
        {
            var result = await _ticketService.GetTicketByIdAsync(ticketId);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }


        // ===============================
        // UPDATE
        // ===============================
        [Authorize(Roles = "MANAGER,STAFF")]
        [HttpPut("")]
        public async Task<IActionResult> Update([FromBody] TicketUpdateRequest request)
        {
            

            var result = await _ticketService.UpdateTicketAsync(request);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
