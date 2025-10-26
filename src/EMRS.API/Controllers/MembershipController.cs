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
        private readonly IAccountService _accountService;

        public MembershipController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateMembership([FromBody] CreateMembershipRequest request)
        {

            var result = await _accountService.CreateMembership(request);
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
