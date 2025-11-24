using EMRS.Application.Abstractions;
using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.RentalContractDTOs;
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
        private readonly IPdfGeneratorService _pdfGeneratorService;
        public RentalController(IRentalService rentalService,IPdfGeneratorService pdfGeneratorService)

        {
            _pdfGeneratorService= pdfGeneratorService;
            _rentalService = rentalService;
        }
        ///RECEIPT
        [HttpPost("receipt/change/vehicle")]
        public async Task<IActionResult> CreateRentalReceiptToChangeVehicle([FromForm] RentalReceiptCreateVehicleChangingRequest rentalReceiptCreateRequest)
        {

            var result = await _rentalService.CreateRentailReceiptForChangingAsync(rentalReceiptCreateRequest);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


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
       
        /// <summary>
        /// Generate PDF preview from template + dynamic params
        /// </summary>


        [HttpGet("receipt/by/{RentalReceiptId}")]
        public async Task<IActionResult> GetById(Guid RentalReceiptId)
        {

            var result = await _rentalService.GetRentalReceiptDetailByIdAsync(RentalReceiptId);
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

            var result = await _rentalService.GetRentalReceiptDetailByBookingIdAsync(bookingId);
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
        [HttpDelete("receipt/{RentalReceiptId}")]
        public async Task<IActionResult> Delete(Guid RentalReceiptId)
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
        [HttpPost("contract/generate")]
        public IActionResult GeneratePdf([FromBody] PdfGenerateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TemplateBase64))
                return BadRequest("Template file (base64) is required.");

            byte[] templateBytes;
            try
            {
                templateBytes = Convert.FromBase64String(request.TemplateBase64);
            }
            catch
            {
                return BadRequest("TemplateBase64 is not a valid Base64 string.");
            }

            byte[] pdfBytes = _pdfGeneratorService.GeneratePdf(templateBytes, request.Parameters);

            return File(pdfBytes, "application/pdf", "preview.pdf");
        }
        //////CONTRACT
        [HttpPost("contract/{rentalContractId}/send-otp-code")]
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
        [HttpPost("contract/create")]
        public async Task<IActionResult> CreateRentalContract([FromForm]RentalContractCreateRequest request)
        {

            var result = await _rentalService.CreateRentalContractAsync( request);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpPost("contract/{rentalContractId}/{rentalReceiptId}/{otpCode}/confirm")]
        public async Task<IActionResult> Create(Guid rentalContractId, Guid rentalReceiptId, string otpCode)
        {

            var result = await _rentalService.ConfirmedRentalContract(rentalContractId, rentalReceiptId, otpCode);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpPost("contract/{bookingId}/{rentalReceiptId}")]
        public async Task<IActionResult> CreateRentalContract(Guid bookingId, Guid rentalReceiptId)
        {

            var result = await _rentalService.CreateRentalContractPdfByGenerateAsync(bookingId, rentalReceiptId);
            if (result.Success)
            {

                return File(result.Data.FileData, "application/pdf", result.Data.Name);
            }
            else
            {
                return BadRequest(result);
            }


        }
     /*   [HttpPost("contract/template")]
        public async Task<IActionResult> CreateRentalwithTemplateContract([FromForm]CreateRentalContractRequest request)
        {

            var result = await _rentalService.CreateRentalContractPdfByGenerateAsync(request.BookingId,request.RentalReceiptId);
            if (result.Success)
            {

                return File(result.Data.FileData, "application/pdf", result.Data.Name);
            }
            else
            {
                return BadRequest(result);
            }


        }*/
        [HttpGet("contract/{bookingId}")]
        public async Task<IActionResult> FindRentalContract(Guid bookingId)
        {

            var result = await _rentalService.GetContractAsync(bookingId);
            if (result.Success)
            {

                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }

        [HttpGet("contract")]
        public async Task<IActionResult> GetAllContracts()
        {

            var result = await _rentalService.GetAllRentalContractsAsync();
            if (result.Success)
            {

                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpPut("contract")]
        public async Task<IActionResult> UpdateRentalContract([FromForm]UpdateRentalContractRequest request)
        {

            var result = await _rentalService.UpdateRentalContractAsync(request);
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

            var result = await _rentalService.DeleteContractAsync(rentalContractId);
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
