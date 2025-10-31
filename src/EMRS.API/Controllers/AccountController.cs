using EMRS.Application.DTOs.AccountDTOs;
using EMRS.Application.DTOs.MembershipDTOs;
using EMRS.Application.DTOs.RenterDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
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
        //ACCOUNT
        [HttpGet("")]
        public async Task<IActionResult> GetALL()
        {

            var result = await _accountService.GetAllAccountAsync();
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }
        //Renter
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
        [HttpGet("renter/{renterId}")]
        public async Task<IActionResult> GetById(Guid renterId  )
        {

            var result = await _accountService.GetRenterDetail(renterId);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }
        [HttpPost("renter/scan")]
        public async Task<IActionResult> Scan([FromForm] RenterScanRequest req)
        {

            var result = await _accountService.ScanAndReturnRenterInfo(req.image);
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
