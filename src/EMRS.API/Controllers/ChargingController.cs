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


    [Authorize(Roles = "STAFF,ADMIN")]
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


    [Authorize(Roles = "STAFF,ADMIN")]
    [HttpPost("get-charging-rate")]
    public async Task<IActionResult> GetChargingRate([FromBody] ChargingRateRequest request)
    {
        var result = await _chargingService.GetChargingRate(request);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [Authorize(Roles = "STAFF,ADMIN")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateChargingRecord([FromBody] ChargingRecordCreateRequest request)
    {
        var result = await _chargingService.CreateChargingRecord(request);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }


    [Authorize(Roles = "RENTER")]
    [HttpGet("renter/get-charging-history")]
    public async Task<IActionResult> GetChargingRecordsByRenter()
    {
        var result = await _chargingService.GetChargingRecordsByRenter();

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet("booking/{bookingId}")]
    public async Task<IActionResult> GetChargingRecordsByBookingId(Guid bookingId)
    {
        var result = await _chargingService.GetChargingRecordsByBookingId(bookingId);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }
}