using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.DTOs.DocumentDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService document)
        {
            _documentService = document;
        }
        [Authorize(Roles = "RENTER")]
  
        [HttpPost("citizen")]
        public async Task<IActionResult> Create([FromForm] DocumentsCreateRequest request)
        {

            var result = await _documentService.CreateUDocument(request);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }

        [HttpPost("driving")]
        public async Task<IActionResult> CreateDriving([FromForm] DocumentsCreateRequest request)
        {

            var result = await _documentService.CreateDrivingDocument(request);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpPut("citizen")]
        public async Task<IActionResult> Updaate([FromForm] DocumentsUpdateRequest request)
        {

            var result = await _documentService.UpdateCitizenDocumentAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpPut("driving")]
        public async Task<IActionResult> UpdateDriving([FromForm] DocumentsUpdateRequest request)
        {

            var result = await _documentService.UpdateDrivingDocumentAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }

        [HttpDelete("{documentId}")]
        public async Task<IActionResult> Delete(Guid documentId)
        {

            var result = await _documentService.DeleteDocumentAsync(documentId);
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
