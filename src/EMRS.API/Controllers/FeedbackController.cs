using EMRS.Application.DTOs.FeedbackDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }
        // -------------------------------------------------------------
        // CREATE FEEDBACK
        // -------------------------------------------------------------
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] FeedbackRequest request)
        {
            var result = await _feedbackService.CreateFeedback(request);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // -------------------------------------------------------------
        // GET FEEDBACK BY BOOKING ID
        // -------------------------------------------------------------
        [HttpGet("booking/{bookingid}")]
        public async Task<IActionResult> GetByBooking(Guid bookingid)
        {
            var result = await _feedbackService.GetFeedbackByBookingId(bookingid);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // -------------------------------------------------------------
        // GET FEEDBACK BY VEHICLE MODEL ID
        // -------------------------------------------------------------
        [HttpGet("vehicle/model/{vehicleModelid}")]
        public async Task<IActionResult> GetByVehicleModel( Guid vehicleModelid)
        {
            var result = await _feedbackService.GetFeedbackByVehicleModelId(vehicleModelid);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // -------------------------------------------------------------
        // GET ALL FEEDBACKS
        // -------------------------------------------------------------
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _feedbackService.GetAllFeedbacks();

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
    }
}
