using EMRS.Application.Common;
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
       
        [HttpPut("")]
        public async Task<IActionResult> Update([FromForm] VehicleUpdateRequest request)
        {

            var result = await _vehicleService.UpdateVehicleByIdAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }
        [HttpDelete("{vehicleId}")]
        public async Task<IActionResult> SoftDeleteAsync(Guid vehicleId)
        {

            var result = await _vehicleService.SoftDeleteVehicleAsync(vehicleId);
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
        public async Task<IActionResult> GetAllVehicle(  string? LicensePlate, string? Color,  decimal? CurrentOdometerKm,
        decimal? BatteryHealthPercentage, string? Status,
        Guid? BranchId, Guid? VehicleModelId
            , int PageSize, int PageNum)
        {
            var request = new VehicleSearchRequest
            {
                LicensePlate = LicensePlate,
                Color = Color,
                CurrentOdometerKm = CurrentOdometerKm,
                BatteryHealthPercentage = BatteryHealthPercentage,
                Status = Status,
                BranchId = BranchId,
                VehicleModelId = VehicleModelId
            };
            var result = await _vehicleService.GetAllVehicleAsync(request,PageSize,PageNum );
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
        [HttpGet("{vehicleId}")]
        public async Task<IActionResult> GetVehicleDetail(Guid vehicleId)
        {

            var result = await _vehicleService.GetVehicleDetailAsync(vehicleId);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }
        [HttpGet("tracking/{vehicleId}")]
        public async Task<IActionResult> GetVehicleTrackingToken(Guid vehicleId)
        {

            var result = await _vehicleService.GetVehicleTrackingTokenAndSignature(vehicleId);
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
        [HttpGet("model/list/{branchId}")]
        public async Task<IActionResult> GetAllVehicleModelBybranchId(Guid branchId)
        {

            var result = await _vehicleService.GetAllVehicleModelByBranchId(branchId);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }
        [HttpGet("model/{branchId}")]
        public async Task<IActionResult> GetAllVehicleModelByBranchIdWithAllVehicle(
     Guid branchId,
     [FromQuery] int pageNum = 1,
     [FromQuery] int pageSize = 10,
     [FromQuery] bool descendingOrder = false)
        {
            if (pageNum <= 0 || pageSize <= 0)
            {
                return BadRequest(ResultResponse<string>.Failure(
                    "PageNum và PageSize phải lớn hơn 0."));
            }

            var result = await _vehicleService
                .GetVehicleModelsWithVehiclesPaginationAsync(
                    branchId, pageSize, pageNum, descendingOrder);

            return result.Success
                ? Ok(result)
                : BadRequest(result);
        }

        [HttpGet("model/search/pagination")]
        public async Task<IActionResult> SearchVehicleWithConds(int PageNum,int PageSize,DateTime? startTime,DateTime? endTime,string? branchId)
        {
            Guid? branchGuid = null;
            if (!string.IsNullOrEmpty(branchId))
            {
                if (Guid.TryParse(branchId, out var parsedId))
                    branchGuid = parsedId;
                else
                    return BadRequest("Invalid branchId format.");
            }
            var criteria = new VehicleModelSearchRequest
            {
                StartDate = startTime,
                EndDate = endTime,
                BranchId = branchGuid
            };
            var result = await _vehicleService.SearchWithTimeSpanForVehicleModels(criteria,PageSize,PageNum);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }
        [HttpGet("model/search/")]
        public async Task<IActionResult> SearchVehicleWithNoPagination( DateTime? startTime, DateTime? endTime, string? branchId)
        {
            Guid? branchGuid = null;
            if (!string.IsNullOrEmpty(branchId))
            {
                if (Guid.TryParse(branchId, out var parsedId))
                    branchGuid = parsedId;
                else
                    return BadRequest("Invalid branchId format.");
            }
            var criteria = new VehicleModelSearchRequest
            {
                StartDate = startTime,
                EndDate = endTime,
                BranchId = branchGuid
            };
            var result = await _vehicleService.SearchWithTimeSpanForVehicleModelsNoPagination(criteria);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }
        [HttpPut("model")]
        public async Task<IActionResult> UpdateModel([FromBody]  UpdateVehicleModelRequest updateVehicleModelRequest)
        {

            var result = await _vehicleService.UpdateVehicleModelAsync(updateVehicleModelRequest);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }
        [HttpDelete("model/{vehicleModelId}")]
        public async Task<IActionResult> SoftDeleteModelAsync(Guid vehicleModelId)
        {

            var result = await _vehicleService.SoftDeleteVehicleModelAsync(vehicleModelId);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

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
