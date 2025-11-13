using EMRS.Application.DTOs.VehicleTransferDTOs;
using EMRS.Application.DTOs.VehicleTransferDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleTransferOrderController : ControllerBase
    {
        private readonly IVehicleTransferOrderService _transferOrderService;

        public VehicleTransferOrderController(
            IVehicleTransferOrderService transferOrderService)
        {
            _transferOrderService = transferOrderService;
        }

        // POST: api/vehicletransferorder/create
        [Authorize(Roles = "ADMIN")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder(
            [FromBody] VehicleTransferOrderCreateRequest request)
        {
            var result = await _transferOrderService.CreateTransferOrder(request);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // PUT: api/vehicletransferorder/{orderId}/dispatch
        [Authorize(Roles = "MANAGER")]
        [HttpPut("{orderId}/dispatch")]
        public async Task<IActionResult> ConfirmDispatched(Guid orderId)
        {
            var result = await _transferOrderService.ConfirmVehicleDispatched(orderId);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // PUT: api/vehicletransferorder/{orderId}/receive
        [Authorize(Roles = "MANAGER")]
        [HttpPut("{orderId}/receive")]
        public async Task<IActionResult> ConfirmReceived(Guid orderId)
        {
            var result = await _transferOrderService.ConfirmVehicleReceived(orderId);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // GET: api/vehicletransferorder
        [Authorize(Roles = "ADMIN,MANAGER")]
        [HttpGet("")]
        public async Task<IActionResult> GetAllOrders()
        {
            var result = await _transferOrderService.GetAllOrders();
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // GET: api/vehicletransferorder/branch/{branchId}
        [Authorize(Roles = "MANAGER")]
        [HttpGet("branch/{branchId}")]
        public async Task<IActionResult> GetOrdersByBranch(Guid branchId)
        {
            var result = await _transferOrderService.GetOrdersByBranch(branchId);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // GET: api/vehicletransferorder/branch/{branchId}/pending
        [Authorize(Roles = "MANAGER")]
        [HttpGet("branch/{branchId}/pending")]
        public async Task<IActionResult> GetPendingOrdersByBranch(Guid branchId)
        {
            var result = await _transferOrderService.GetPendingOrdersByBranch(branchId);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // GET: api/vehicletransferorder/intransit
        [Authorize(Roles = "ADMIN,MANAGER")]
        [HttpGet("intransit")]
        public async Task<IActionResult> GetInTransitOrders()
        {
            var result = await _transferOrderService.GetInTransitOrders();
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // GET: api/vehicletransferorder/{orderId}
        [Authorize(Roles = "ADMIN,MANAGER")]
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderDetail(Guid orderId)
        {
            var result = await _transferOrderService.GetOrderDetail(orderId);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // PUT: api/vehicletransferorder/{orderId}/cancel
        [Authorize(Roles = "ADMIN")]
        [HttpPut("{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid orderId)
        {
            var result = await _transferOrderService.CancelOrder(orderId);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
    }
}