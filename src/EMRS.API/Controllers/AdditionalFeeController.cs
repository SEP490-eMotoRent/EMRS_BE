using EMRS.Application.DTOs.AdditionalFeeDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/additional-fees")]
    [ApiController]
    [Authorize(Roles = "STAFF,MANAGER")]
    public class AdditionalFeeController : ControllerBase
    {
        private readonly IAdditionalFeeService _additionalFeeService;

        public AdditionalFeeController(IAdditionalFeeService additionalFeeService)
        {
            _additionalFeeService = additionalFeeService;
        }

        /// <summary>
        /// Tính và thêm phí trả xe trễ (tự động)
        /// </summary>
        [HttpPost("late-return")]
        public async Task<IActionResult> AddLateReturnFee([FromBody] AddLateReturnFeeRequest request)
        {
            var result = await _additionalFeeService.AddLateReturnFeeAsync(request);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        /// <summary>
        /// Thêm phí vệ sinh (cố định 50k)
        /// </summary>
        [HttpPost("cleaning")]
        public async Task<IActionResult> AddCleaningFee([FromBody] AddCleaningFeeRequest request)
        {
            var result = await _additionalFeeService.AddCleaningFeeAsync(request);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        /// <summary>
        /// Tính và thêm phí trả xe khác chi nhánh (tự động)
        /// </summary>
        [HttpPost("cross-branch")]
        public async Task<IActionResult> AddCrossBranchFee([FromBody] AddCrossBranchFeeRequest request)
        {
            var result = await _additionalFeeService.AddCrossBranchFeeAsync(request);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        /// <summary>
        /// Tính và thêm phí vượt km (tự động)
        /// </summary>
        [HttpPost("excess-km")]
        public async Task<IActionResult> AddExcessKmFee([FromBody] AddExcessKmFeeRequest request)
        {
            var result = await _additionalFeeService.AddExcessKmFeeAsync(request);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        /// <summary>
        /// Thêm phí hư hỏng (Staff chọn loại và nhập số tiền)
        /// </summary>
        [HttpPost("damage")]
        public async Task<IActionResult> AddDamageFee([FromBody] AddDamageFeeRequest request)
        {
            var result = await _additionalFeeService.AddDamageFeeAsync(request);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        /// <summary>
        /// Lấy danh sách các loại hư hỏng (cho dropdown)
        /// </summary>
        [HttpGet("damage-types")]
        public async Task<IActionResult> GetDamageTypes()
        {
            var result = await _additionalFeeService.GetDamageTypesAsync();

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        /// <summary>
        /// Lấy tất cả phí bổ sung của một booking
        /// </summary>
        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetFeesByBookingId(Guid bookingId)
        {
            var result = await _additionalFeeService.GetFeesByBookingIdAsync(bookingId);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        /// <summary>
        /// Xóa một phí bổ sung
        /// </summary>
        [HttpDelete("{feeId}")]
        public async Task<IActionResult> DeleteFee(Guid feeId)
        {
            var result = await _additionalFeeService.DeleteFeeAsync(feeId);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
    }
}
