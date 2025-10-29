using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.RentalReceiptDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/rental")]
    [ApiController]
    public class RentalController : ControllerBase
    {
        private readonly IRentalService _rentalService;
        public RentalController(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }
        [HttpPost("receipt")]
        public async Task<IActionResult> Create([FromForm]  RentalReceiptCreateRequest rentalReceiptCreateRequest)
        {

            var result = await _rentalService.CreateRentailReceiptAsync(rentalReceiptCreateRequest);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpPost("receipt/{rentalContractId}/send-otp-code")]
        public async Task<IActionResult> SendingOtpCode(Guid rentalContractId)
        {

            var result = await _rentalService.SendRenterCodeForOtpSignAsync(rentalContractId);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpPost("contract/{bookingId}")]
        public async Task<IActionResult> CreateRentalContract(Guid bookingId)
        {

            var result = await _rentalService.CreateRentalContractAsync(bookingId);
            if (result.Success&&result.Data!=null)
            {

                return File(result.Data.FileData, "application/pdf", result.Data.Name);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpGet("contract/{rentalContractId}")]
        public async Task<IActionResult> FindRentalContract(Guid rentalContractId)
        {

            var result = await _rentalService.GetContractAsync(rentalContractId);
            if (result.Success)
            {

                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpDelete("contract/{rentalContractId}")]
        public async Task<IActionResult> DeleteRentalContract(Guid rentalContractId)
        {

            var result = await _rentalService.DeleteRentalReceiptAsync(rentalContractId);
            if (result.Success)
            {

                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpDelete("receipt/{RentalReceiptId}")]
        public async Task<IActionResult> Create(Guid RentalReceiptId)
        {

            var result = await _rentalService.DeleteRentalReceiptAsync(RentalReceiptId);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpPost("receipt/{RentalReceiptId}/{otpCode}/confirm")]
        public async Task<IActionResult> Create(Guid RentalReceiptId,string otpCode)
        {

            var result = await _rentalService.ConfirmedRentalReceipt(RentalReceiptId,otpCode);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpGet("receipt/{bookingId}")]
        public async Task<IActionResult> GetByBookingid(Guid bookingId)
        {

            var result = await _rentalService.GetAllByBookingIdAsync(bookingId);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpGet("receipt")]
        public async Task<IActionResult> GetAll()
        {

            var result = await _rentalService.GetAllRentalReceipt();
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
