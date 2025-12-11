using EMRS.Application.DTOs.MediaDTOs;
using EMRS.Application.DTOs.VehicleDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [HttpPut("")]
        public async Task<IActionResult> UpdateVehicleAndModel([FromForm] MediaUpdateRequest mediaUpdateRequest)
        {

            var result = await _mediaService.UpdateSingleMediaAsync( mediaUpdateRequest);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }
        [HttpDelete("")]
        public async Task<IActionResult> Delete([FromQuery] Guid mediaId)
        {
            var result = await _mediaService.DeleteMediaAsync(mediaId);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("")]
        public async Task<IActionResult> Add([FromForm] AddMediaRequest addMediaRequest)
        {
            if (!Enum.IsDefined(typeof(MediaEntityTypeEnum), addMediaRequest.EntityType))
                return BadRequest("Invalid EntityType");

            if (!Enum.IsDefined(typeof(MediaTypeEnum), addMediaRequest.MediaType))
                return BadRequest("Invalid MediaType");

            var result = await _mediaService.AddMediaAsync(addMediaRequest);
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
