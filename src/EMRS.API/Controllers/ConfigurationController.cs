using EMRS.Application.Common;
using EMRS.Application.DTOs.ConfigurationDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _configurationService;

        public ConfigurationController(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _configurationService.GetAllAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _configurationService.GetByIdAsync(id);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ConfigurationCreateRequest config)
        {
            var result = await _configurationService.CreateAsync(config);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Configuration config)
        {
            if (id != config.Id)
                return BadRequest(ResultResponse<Configuration>.Failure("ID mismatch"));

            var result = await _configurationService.UpdateAsync(config);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _configurationService.DeleteAsync(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("face")]
        public async Task<ActionResult> CreateFaceSet()
        {
            var result = await _configurationService.CreateFaceSet();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

     

        [HttpDelete("face/{facesetToken}")]
        public async Task<ActionResult> DeleteFaceSet(string facesetToken)
        {
            var result = await _configurationService.RemoveFaceSet(facesetToken);
            return Ok(result);
        }
    }
}
