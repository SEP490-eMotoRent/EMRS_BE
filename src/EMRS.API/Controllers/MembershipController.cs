using EMRS.Application.Common;
using EMRS.Application.DTOs.MembershipDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/membership")]
    [ApiController]
    public class MembershipController : ControllerBase
    {
        private readonly IMembershipService _membershipService;

        public MembershipController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateMembership([FromBody] CreateMembershipRequest request)
        {

            var result = await _membershipService.CreateMembership(request);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllMemberships()
        {
            var result = await _membershipService.GetAllMembershipsAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
    
        public async Task<IActionResult> GetMembershipById(Guid id)
        {
            var result = await _membershipService.GetMembershipByIdAsync(id);

            if (!result.Success)
            {
                return result.Message.Contains("not found") || result.Message.Contains("Không tìm thấy")
                    ? NotFound(result)
                    : BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("update")]
       
        public async Task<IActionResult> UpdateMembership([FromBody] UpdateMembershipRequest request)
        {
          
            var result = await _membershipService.UpdateAsync(request);

            if (!result.Success)
            {
                return result.Message.Contains("not found") || result.Message.Contains("Không tìm thấy")
                    ? NotFound(result)
                    : BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
       
        public async Task<IActionResult> DeleteMembership(Guid id)
        {
            var result = await _membershipService.DeleteAsync(id);

            if (!result.Success)
            {
                return result.Message.Contains("not found") || result.Message.Contains("Không tìm thấy")
                    ? NotFound(result)
                    : BadRequest(result);
            }

            return Ok(result);
        }
    }
}
