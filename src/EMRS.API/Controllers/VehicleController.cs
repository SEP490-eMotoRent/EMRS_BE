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
        public async Task<IActionResult> CreateModel([FromForm] VehicleModelCreateRequest request)
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
        [HttpGet("")]
        public async Task<IActionResult> GetAllVehicle()
        {

            var result = await _vehicleService.GetAllVehicleAsync();
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
        public async Task<IActionResult> CreateVehicle([FromForm] CreateVehicleRequest request)
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
        [HttpGet("model/list")]
        public async Task<IActionResult> GetAllVehicleModel()
        {

            var result = await _vehicleService.GetAllVehicleModel();
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }
        [HttpGet("model/detail/{id:guid}")]
        public async Task<IActionResult> GetVehicleModelDetail(Guid id)
        {
            var result = await _vehicleService.GetVehicleModelByIdAsync(id);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

    }
}
