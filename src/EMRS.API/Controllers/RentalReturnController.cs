// EMRS.API/Controllers/RentalReceiptController.cs

using EMRS.Application.DTOs.RentalReceiptDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/rental-return")]
[ApiController]
public class RentalReturnController : ControllerBase
{
    private readonly IRentalReturnService _rentalReceiptService;

    public RentalReturnController(IRentalReturnService rentalReceiptService)
    {
        _rentalReceiptService = rentalReceiptService;
    }



    [Authorize(Roles = "STAFF")]
    [HttpPost("return/scan-and-init")]
    [Consumes("multipart/form-data")] 
    public async Task<IActionResult> InitiateReturn(IFormFile faceImage)
    {
        if (faceImage == null || faceImage.Length == 0)
        {
            return BadRequest(new { success = false, message = "Face image is required" });
        }

        var result = await _rentalReceiptService.InitiateReturnProcessAsync(faceImage);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }



    [Authorize(Roles = "STAFF")]
    [HttpPost("return/upload-and-analyze")]
    [Consumes("multipart/form-data")] 
    public async Task<IActionResult> UploadAndAnalyze([FromForm] UploadReturnImagesRequest request)
    {
        if (request.ReturnImages == null || request.ReturnImages.Count != 4)
        {
            return BadRequest(new
            {
                success = false,
                message = "Exactly 4 return images are required (front, back, left, right)"
            });
        }

        var result = await _rentalReceiptService.UploadAndAnalyzeReturnImagesAsync(request);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }



    [Authorize(Roles = "STAFF")]
    [HttpPost("return/create-receipt")]
    [Consumes("multipart/form-data")] 
    public async Task<IActionResult> CreateReturnReceipt([FromForm] CreateReturnReceipt request)
    {
        var result = await _rentalReceiptService.CreateReturnReceiptAsync(request);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }



    [Authorize(Roles = "STAFF,RENTER")]
    [HttpPut("return/finalize")]
    public async Task<IActionResult> FinalizeReturn([FromBody] FinalizeReturn request)
    {
        var result = await _rentalReceiptService.FinalizeReturnAsync(request);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }


    [Authorize(Roles = "STAFF, RENTER")]
    [HttpGet("return/{bookingId}/summary")]
    public async Task<IActionResult> GetSettlementSummary(Guid bookingId)
    {
        var result = await _rentalReceiptService.GetSettlementSummaryAsync(bookingId);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [Authorize(Roles = "STAFF,MANAGER")]
    [HttpPut("return/update")]
    public async Task<IActionResult> UpdateReturnReceipt([FromForm] UpdateReturnReceiptRequest request)
    {
        var result = await _rentalReceiptService.UpdateReturnReceiptAsync(request);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [Authorize(Roles = "STAFF,MANAGER,ADMIN")]
    [HttpDelete("return/{bookingId}")]
    public async Task<IActionResult> DeleteReturnReceipt(Guid bookingId)
    {
        var result = await _rentalReceiptService.DeleteReturnReceiptAsync(bookingId);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [Authorize(Roles = "STAFF")]
    [HttpPost("return/vehicle-swap")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ReturnForVehicleSwap([FromForm] ReturnForVehicleSwapRequest request)
    {
        var result = await _rentalReceiptService.ReturnForVehicleSwapAsync(request);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

}