using EMRS.Application.DTOs.AccountDTOs;
using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchController : ControllerBase
    {
        private readonly IBranchService _branchService;

        public BranchController(IBranchService branchService)
        {
            _branchService = branchService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateBranchRequest request)
        {

            var result = await _branchService.CreateABranch(request);
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

            var result = await _branchService.GetAllBranches();
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpGet("{vehicleModelId}")]
        public async Task<IActionResult> GetAllBranchesWithSameModelId( Guid vehicleModelId)
        {

            var result = await _branchService.GetAllBranchesWithSameModelIdAsync(vehicleModelId);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpGet("search")]
        public async Task<IActionResult> Search(DateTime? startDate, DateTime? endDate)
        {
            var branchSearchRequest = new BranchSearchRequest
            {
                StartDate = startDate,
                EndDate = endDate
            };
            var result = await _branchService.SearchWithTimeSpanForBranch(branchSearchRequest);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpGet("charging/search/{lat}/{lon}/{radius}")]
        public async Task<IActionResult> GetNearbyChargingBrances(double lat, double lon,double radius)
        {

            var result = await _branchService.GetNearbyBranchesAsync(lat,lon, radius);
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
