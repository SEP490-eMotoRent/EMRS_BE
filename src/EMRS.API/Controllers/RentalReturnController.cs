// EMRS.API/Controllers/RentalReceiptController.cs

using EMRS.Application.DTOs.RentalReceiptDTOs;
using EMRS.Application.Interfaces.Services;
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


    /// <summary>
    /// Khởi tạo quy trình trả xe bằng cách scan khuôn mặt renter
    /// </summary>
    [Authorize(Roles = "STAFF")]
    [HttpPost("return/scan-and-init")]
    [Consumes("multipart/form-data")] // ✅ THÊM
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


    /// <summary>
    /// Upload ảnh xe lúc trả và phân tích bằng AI
    /// </summary>
    [Authorize(Roles = "STAFF")]
    [HttpPost("return/upload-and-analyze")]
    [Consumes("multipart/form-data")] // ✅ THÊM
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


    /// <summary>
    /// Tạo biên bản trả xe với các chi phí phát sinh
    /// </summary>
    [Authorize(Roles = "STAFF")]
    [HttpPost("return/create-receipt")]
    [Consumes("multipart/form-data")] // ✅ THÊM
    public async Task<IActionResult> CreateReturnReceipt([FromForm] CreateReturnReceiptRequest request)
    {
        var result = await _rentalReceiptService.CreateReturnReceiptAsync(request);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }


    /// <summary>
    /// Hoàn tất quy trình trả xe và xử lý thanh toán
    /// </summary>
    [Authorize(Roles = "STAFF,RENTER")]
    [HttpPut("return/finalize")]
    public async Task<IActionResult> FinalizeReturn([FromBody] FinalizeReturnRequest request)
    {
        var result = await _rentalReceiptService.FinalizeReturnAsync(request);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    /// <summary>
    /// Lấy tóm tắt quyết toán để renter xem trước khi xác nhận
    /// </summary>
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

}