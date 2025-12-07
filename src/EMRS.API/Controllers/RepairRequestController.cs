using EMRS.Application.Abstractions;
using EMRS.Application.DTOs.RepairRequestDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepairRequestController : ControllerBase
    {
        private readonly IRepairRequestService _repairRequestService;
        public RepairRequestController( IRepairRequestService repairRequestService)
        {
            _repairRequestService = repairRequestService;
        }
        [Authorize(Roles = nameof(UserRoleName.MANAGER))]
        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] RepairRequestCreateRequest request)
        {
            var result = await _repairRequestService.CreateRepairRequestAsync(request);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
        [Authorize(Roles = nameof(UserRoleName.ADMIN))]

        [HttpGet("")]
        public async Task<IActionResult> GetAll(
            int pageNum = 1,
            int pageSize = 10,
            bool orderByDesc = false)
        {
            var result = await _repairRequestService.GetAllAsync(pageNum, pageSize, orderByDesc);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
        [Authorize(Roles = nameof(UserRoleName.MANAGER))]

        [HttpGet("branch")]
        public async Task<IActionResult> GetByBranch(
    int pageNum = 1,
    int pageSize = 10,
    bool orderByDesc = false)
        {
            var result = await _repairRequestService.GetByBranchIdAsync(pageNum, pageSize, orderByDesc);

            if (result.Success)
                return Ok(result);
      
            else
                return BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _repairRequestService.GetByIdAsync(id);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
        [Authorize(Roles = nameof(UserRoleName.TECHNICIAN))]
        [HttpGet("technician/{technicianId}")]
        public async Task<IActionResult> GetByTechnicianId(
            Guid technicianId,
            int pageNum = 1,
            int pageSize = 10,
            bool orderByDesc = false)
        {
            var result = await _repairRequestService.GetByTechnicianIdAsync(
                technicianId, pageNum, pageSize, orderByDesc);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
        [Authorize(Roles = nameof(UserRoleName.TECHNICIAN))]
        [HttpPost("technician")]
        public async Task<IActionResult> CreateRequest([FromBody] RepairRequestTechnicianCreateRequest request)
        {
            var result = await _repairRequestService.CreateRepairRequestForTechnicianAsync(request);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
        [Authorize(Roles = nameof(UserRoleName.TECHNICIAN))]

        [HttpPut("technician")]
        public async Task<IActionResult> Update([FromBody] UpdateRepairRequestTechnician request)
        {
            var result = await _repairRequestService.UpdateRepairRequestTechnicianAsync(request);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
        [Authorize(Roles = nameof(UserRoleName.ADMIN))]

        [HttpPut("")]
        public async Task<IActionResult> Update([FromBody] RepairRequestUpdateRequest request)
        {
            var result = await _repairRequestService.UpdateRepairRequestAsync(request);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

    }
}
