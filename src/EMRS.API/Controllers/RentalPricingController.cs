using EMRS.Application.DTOs.RentalPricingDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/rental/pricing")]
    [ApiController]
    public class RentalPricingController : ControllerBase
    {
        private IVehicleService _vehicleService;
        public RentalPricingController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }
        [HttpPost("")]
        public async Task<IActionResult> CreatePricing([FromBody] CreateRentalPricingRequest request)
        {

            var result = await _vehicleService.CreateRentalPricing(request);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {

            var result = await _vehicleService.GetAllRentalPricing();
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
