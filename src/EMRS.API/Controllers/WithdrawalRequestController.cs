// EMRS.API/Controllers/WithdrawalRequestController.cs
using EMRS.Application.DTOs.WithdrawalRequestDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WithdrawalRequestController : ControllerBase
{
    private readonly IWithdrawalRequestService _withdrawalRequestService;

    public WithdrawalRequestController(IWithdrawalRequestService withdrawalRequestService)
    {
        _withdrawalRequestService = withdrawalRequestService;
    }

    // ==================== RENTER ENDPOINTS ====================

    /// <summary>
    /// POST /api/withdrawalrequest/create - Renter creates withdrawal request
    /// </summary>
    [Authorize(Roles = "RENTER")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] WithdrawalRequestCreateRequest request)
    {
        var result = await _withdrawalRequestService.CreateWithdrawalRequest(request);
        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    /// <summary>
    /// GET /api/withdrawalrequest/my-requests - Renter gets their withdrawal requests
    /// </summary>
    [Authorize(Roles = "RENTER")]
    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests()
    {
        var result = await _withdrawalRequestService.GetMyWithdrawalRequests();
        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    /// <summary>
    /// GET /api/withdrawalrequest/{id} - Get withdrawal request detail
    /// </summary>
    [Authorize(Roles = "RENTER,ADMIN")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var result = await _withdrawalRequestService.GetWithdrawalRequestDetail(id);
        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    /// <summary>
    /// PUT /api/withdrawalrequest/{id}/cancel - Renter cancels their withdrawal request
    /// </summary>
    [Authorize(Roles = "RENTER")]
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _withdrawalRequestService.CancelWithdrawalRequest(id);
        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    // ==================== ADMIN ENDPOINTS (CHANGED FROM MANAGER) ====================

    /// <summary>
    /// GET /api/withdrawalrequest - Admin gets all withdrawal requests with filters
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpGet("")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? Status,
        [FromQuery] DateTime? FromDate,
        [FromQuery] DateTime? ToDate,
        [FromQuery] Guid? WalletId,
        [FromQuery] int PageNum = 1,
        [FromQuery] int PageSize = 10)
    {
        var searchRequest = new WithdrawalRequestSearchRequest
        {
            Status = Status,
            FromDate = FromDate,
            ToDate = ToDate,
            WalletId = WalletId
        };

        var result = await _withdrawalRequestService.GetAllWithdrawalRequests(searchRequest, PageNum, PageSize);
        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    /// <summary>
    /// PUT /api/withdrawalrequest/{id}/approve - Admin approves withdrawal request (deducts wallet balance)
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _withdrawalRequestService.ApproveWithdrawalRequest(id);
        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    /// <summary>
    /// PUT /api/withdrawalrequest/{id}/reject - Admin rejects withdrawal request
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] string rejectionReason)
    {
        var result = await _withdrawalRequestService.RejectWithdrawalRequest(id, rejectionReason);
        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    /// <summary>
    /// PUT /api/withdrawalrequest/{id}/complete - Admin confirms bank transfer completed
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var result = await _withdrawalRequestService.CompleteWithdrawalRequest(id);
        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }
}