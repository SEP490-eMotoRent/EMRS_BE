using EMRS.Application.DTOs.VehicleModelDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }
        [HttpPost("model/create")]
        public async Task<IActionResult> Create()
        {

            var result = await _walletService.CreateWalletAsync();
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }

        [Authorize(Roles = "RENTER")]
        [HttpGet("my-balance")]
        public async Task<IActionResult> GetMyWalletBalance()
        {
            var result = await _walletService.GetMyWalletBalanceAsync();

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

    }
}
