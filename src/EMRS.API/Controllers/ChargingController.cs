// EMRS.API/Controllers/ChargingController.cs
using EMRS.Application.DTOs.ChargingRecordDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChargingController : ControllerBase
{
    private readonly IChargingService _chargingService;

    public ChargingController(IChargingService chargingService)
    {
        _chargingService = chargingService;
    }

    /// <summary>
    /// API 1: Tìm booking đang thuê xe theo biển số
    /// </summary>
    /// <param name="licensePlate">Biển số xe (VD: 73-MD6999.99)</param>
    /// <returns>Thông tin booking và renter đang thuê xe</returns>
    [Authorize(Roles = "STAFF")]
    [HttpGet("search-by-license-plate")]
    public async Task<IActionResult> SearchBookingByLicensePlate([FromQuery] string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
            return BadRequest(new { success = false, message = "Biển số xe không được để trống" });

        var result = await _chargingService.SearchBookingByLicensePlate(licensePlate.Trim());

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    /// <summary>
    /// API 2: Lấy bảng giá sạc điện theo thời gian
    /// </summary>
    /// <param name="request">Thời gian sạc</param>
    /// <returns>Khung giờ và đơn giá điện</returns>
    [Authorize(Roles = "STAFF")]
    [HttpPost("get-charging-rate")]
    public async Task<IActionResult> GetChargingRate([FromBody] ChargingRateRequest request)
    {
        var result = await _chargingService.GetChargingRate(request);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    /// <summary>
    /// API 3: Tạo phiếu sạc xe
    /// </summary>
    /// <param name="request">Thông tin sạc xe</param>
    /// <returns>Thông tin phiếu sạc đã tạo</returns>
    [Authorize(Roles = "STAFF")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateChargingRecord([FromBody] ChargingRecordCreateRequest request)
    {
        var result = await _chargingService.CreateChargingRecord(request);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }
}