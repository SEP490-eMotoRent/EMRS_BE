using EMRS.Application.Abstractions.Models.VNPay;
using EMRS.Application.DTOs.VehicleModelDTOs;
using EMRS.Application.DTOs.WalletDTOs;
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

        [Authorize(Roles = "RENTER")]
        [HttpPost("topup")]
        public async Task<IActionResult> CreateTopUpRequest([FromBody] WalletTopUpRequest request)
        {
            var result = await _walletService.CreateTopUpRequestAsync(request);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        [HttpPut("vnpay/callback")]
        public async Task<IActionResult> VnPayCallback([FromBody] VNPayResponseData vnPayResponse)
        {
            var result = await _walletService.ProcessTopUpCallbackAsync(vnPayResponse);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

    }
}
