using EMRS.Application.DTOs.HolidayPricingDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidayPricingController : ControllerBase
    {
        private readonly IHolidayPricingService _holidayPricingService;
        public HolidayPricingController(IHolidayPricingService holidayPricingService)
        {
            _holidayPricingService = holidayPricingService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _holidayPricingService.GetAllAsync();
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]HolidayPricingCreateRequest holidayPricingCreateRequest )
        {
            var result = await _holidayPricingService.CreateAsync(holidayPricingCreateRequest);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _holidayPricingService.GetByIdAsync(id);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }
        [HttpGet("current/date")]
        public async Task<IActionResult> GetByCurrentDateAsync()
        {
            var result = await _holidayPricingService.GetByCurrentDateAsync();
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }
        [HttpPut("")]
        public async Task<IActionResult> Update( [FromBody] HolidayPricingUpdateRequest holidayPricingUpdateRequest)
        {
           
            var result = await _holidayPricingService.UpdateAsync(holidayPricingUpdateRequest);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _holidayPricingService.DeleteAsync(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
