using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.RentalPricingDTOs;
using EMRS.Application.DTOs.VehicleDTOs;
using EMRS.Application.DTOs.VehicleModelDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }
        [HttpPost("model/create")]
        public async Task<IActionResult> CreateModel([FromBody] CreateVehicleModelRequest request)
        {

            var result = await _vehicleService.CreateVehicleModel(request);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleRequest request)
        {

            var result = await _vehicleService.CreateVehicle(request);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }
        [HttpPost("pricing/create")]
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
        [HttpGet("list")]
        public async Task<IActionResult> GetAllVehicle()
        {

            var result = await _vehicleService.GetAllVehicles();
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
