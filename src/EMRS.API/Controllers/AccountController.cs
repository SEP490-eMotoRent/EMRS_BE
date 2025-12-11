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
        [HttpGet("{accountId}")]
        public async Task<IActionResult> GetByAccountId([FromRoute] Guid accountId)
        {

            var result = await _accountService.GetAccountDetailAsync(accountId);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }
        [HttpPut("")]
        public async Task<IActionResult> UpdateRole([FromBody]AccountRoleUpdate accountRoleUpdate )
        {

            var result = await _accountService.UpdateAccountRole(accountRoleUpdate);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }
        [HttpDelete("")]
        public async Task<IActionResult> DeleteAccount([FromBody] AccountDeleteUpdate accountDeleteUpdate)
        {

            var result = await _accountService.SoftDeleteAccount(accountDeleteUpdate);
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
        [HttpPost("renter/search/citizen")]
        public async Task<IActionResult> SearchByCitizenId(string citizenId)
        {

            var result = await _accountService.GetRenterByCitizenIdAsync(citizenId);
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
        [HttpDelete("renter/{url}")]
        public async Task<IActionResult> DeleteFaceScan(string url)
        {

            var result = await _accountService.DeleteScanerFace(url);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }

        [HttpPost("create-account")]
        public async Task<IActionResult> CreateAccount([FromBody] AccountCreateRequest request)
        {
            var result = await _accountService.CreateAccount(request);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
    }
}
