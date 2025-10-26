using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.RentalReceiptDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/receipt")]
    [ApiController]
    public class RentalReceiptController : ControllerBase
    {
        private readonly IRentalReceiptService _rentalReceiptService;
        public RentalReceiptController(IRentalReceiptService rentalReceiptService)
        {
            _rentalReceiptService = rentalReceiptService;
        }
        [HttpPost("")]
        public async Task<IActionResult> Create([FromForm]  RentalReceiptCreateRequest rentalReceiptCreateRequest)
        {

            var result = await _rentalReceiptService.CreateRentailReceiptAsync(rentalReceiptCreateRequest);
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
