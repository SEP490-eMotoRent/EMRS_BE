using EMRS.Application.Abstractions;
using EMRS.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlespiController : Controller
    {
        private readonly IFlespiService _flespiService;

        public FlespiController(IFlespiService flespiService)
        {
            _flespiService = flespiService;
        }

        /// <summary>
        /// Lấy thông tin channel của bạn
        /// </summary>
        /// <remarks>
        /// GET /api/FlespiTest/channel/1321205
        /// </remarks>
        [HttpGet("channel/{channelId}")]
        public async Task<IActionResult> GetChannelInfo(int channelId)
        {
            var result = await _flespiService.GetChannelInfoAsync(channelId);
            return Ok(new { data = result });
        }

        /// <summary>
        /// Lấy danh sách TẤT CẢ devices trong tài khoản Flespi
        /// </summary>
        /// <remarks>
        /// GET /api/FlespiTest/devices
        /// </remarks>
        [HttpGet("devices")]
        public async Task<IActionResult> GetAllDevices()
        {
            var result = await _flespiService.GetAllDevicesAsync();
            return Ok(new { data = result });
        }

        /// <summary>
        /// Lấy thông tin device cụ thể
        /// </summary>
        /// <remarks>
        /// GET /api/FlespiTest/device/{deviceId}
        /// 
        /// Bạn cần biết Device ID (không phải IMEI).
        /// Lấy Device ID bằng cách gọi GET /devices trước.
        /// </remarks>
        [HttpGet("device/{deviceId}")]
        public async Task<IActionResult> GetDeviceInfo(int deviceId)
        {
            var result = await _flespiService.GetDeviceInfoAsync(deviceId);
            return Ok(new { data = result });
        }

        /// <summary>
        /// Lấy 10 messages GPS mới nhất
        /// </summary>
        /// <remarks>
        /// GET /api/FlespiTest/device/{deviceId}/messages?count=10
        /// 
        /// Đây chính là dữ liệu GPS bạn đang thấy trên Flespi Panel.
        /// </remarks>
        [HttpGet("device/{deviceId}/messages")]
        public async Task<IActionResult> GetLatestMessages(int deviceId, [FromQuery] int count = 10)
        {
            var result = await _flespiService.GetLatestMessagesAsync(deviceId, count);
            return Ok(new { data = result });
        }



    }
}
