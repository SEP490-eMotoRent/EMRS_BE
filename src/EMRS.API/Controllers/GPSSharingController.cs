using EMRS.Application.DTOs.GPSSharingDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GPSSharingController : ControllerBase
    {
        private readonly IGPSSharingService _service;

        public GPSSharingController(IGPSSharingService service)
        {
            _service = service;
        }

        // ============================================
        // RENTER ENDPOINTS
        // ============================================

        /// <summary>
        /// Owner tạo lời mời GPS sharing cho xe đang thuê
        /// </summary>
       /* [Authorize(Roles = "RENTER")]
        [HttpPost("invite")]
        public async Task<IActionResult> CreateInvitation(
            [FromBody] GPSSharingCreateRequest request)
        {
            var result = await _service.CreateInvitation(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Guest tham gia GPS sharing bằng invitation code
        /// </summary>
        [Authorize(Roles = "RENTER")]
        [HttpPost("join")]
        public async Task<IActionResult> JoinSharing(
            [FromBody] GPSSharingJoinRequest request)
        {
            var result = await _service.JoinSharing(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy chi tiết một session cụ thể
        /// </summary>
        [Authorize(Roles = "RENTER")]
        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetSessionDetail(Guid sessionId)
        {
            var result = await _service.GetSessionDetail(sessionId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Hủy session (Owner hoặc Guest)
        /// </summary>
        [Authorize(Roles = "RENTER")]
        [HttpDelete("session/{sessionId}/cancel")]
        public async Task<IActionResult> CancelSession(Guid sessionId)
        {
            var result = await _service.CancelSession(sessionId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ============================================
        // MANAGER/ADMIN ENDPOINTS
        // ============================================

        /// <summary>
        /// Lấy tất cả GPS sharing sessions (cho Manager/Admin)
        /// </summary>
        [Authorize(Roles = "RENTER,MANAGER,ADMIN")]
        [HttpGet("all-sessions")]
        public async Task<IActionResult> GetAllSessions()
        {
            var result = await _service.GetAllSessions();
            return result.Success ? Ok(result) : BadRequest(result);
        }

*/
    }
}
