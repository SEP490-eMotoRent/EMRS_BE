using EMRS.Application.DTOs.AccountDTOs;
using EMRS.Application.DTOs.MembershipDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EMRS.API.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

 
        [HttpPut("renter")]
        public async Task<IActionResult> UpdateRenterAccount([FromForm] RenterAccountUpdateRequest request)
        {

            var result = await _accountService.UpdateUserProfile(request);
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
