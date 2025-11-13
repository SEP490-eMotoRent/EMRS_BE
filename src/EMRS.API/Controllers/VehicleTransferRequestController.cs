using EMRS.Application.DTOs.VehicleTransferDTOs;
using EMRS.Application.DTOs.VehicleTransferDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleTransferRequestController : ControllerBase
    {
        private readonly IVehicleTransferRequestService _transferRequestService;

        public VehicleTransferRequestController(
            IVehicleTransferRequestService transferRequestService)
        {
            _transferRequestService = transferRequestService;
        }

        // POST: api/vehicletransferrequest/create
        [Authorize(Roles = "MANAGER")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateRequest(
            [FromBody] VehicleTransferRequestCreateRequest request)
        {
            var result = await _transferRequestService.CreateTransferRequest(request);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // GET: api/vehicletransferrequest/pending
        [Authorize(Roles = "ADMIN")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var result = await _transferRequestService.GetAllPendingRequests();
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // GET: api/vehicletransferrequest
        [Authorize(Roles = "ADMIN,MANAGER")]
        [HttpGet("")]
        public async Task<IActionResult> GetAllRequests()
        {
            var result = await _transferRequestService.GetAllRequests();
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // GET: api/vehicletransferrequest/branch/{branchId}
        [Authorize(Roles = "MANAGER")]
        [HttpGet("branch/{branchId}")]
        public async Task<IActionResult> GetRequestsByBranch(Guid branchId)
        {
            var result = await _transferRequestService.GetRequestsByBranch(branchId);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // GET: api/vehicletransferrequest/{requestId}
        [Authorize(Roles = "ADMIN,MANAGER")]
        [HttpGet("{requestId}")]
        public async Task<IActionResult> GetRequestDetail(Guid requestId)
        {
            var result = await _transferRequestService.GetRequestDetail(requestId);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // PUT: api/vehicletransferrequest/{requestId}/approve
        [Authorize(Roles = "ADMIN")]
        [HttpPut("{requestId}/approve")]
        public async Task<IActionResult> ApproveRequest(Guid requestId)
        {
            var result = await _transferRequestService.ApproveTransferRequest(requestId);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // PUT: api/vehicletransferrequest/{requestId}/cancel
        [Authorize(Roles = "ADMIN,MANAGER")]
        [HttpPut("{requestId}/cancel")]
        public async Task<IActionResult> CancelRequest(Guid requestId)
        {
            var result = await _transferRequestService.CancelTransferRequest(requestId);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
    }
}