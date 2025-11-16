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


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePricing(Guid id, [FromBody] UpdateRentalPricingRequest request)
        {
            // Ensure ID from route matches request body
            request.Id = id;

            var result = await _vehicleService.UpdateRentalPricing(request);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePricing(Guid id)
        {
            var result = await _vehicleService.DeleteRentalPricing(id);

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
