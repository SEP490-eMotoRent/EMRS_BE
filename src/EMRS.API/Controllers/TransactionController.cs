using EMRS.Application.DTOs.TransactionDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _transactionService.GetAllAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _transactionService.GetByIdAsync(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TransactionCreateRequest request)
        {
            var result = await _transactionService.CreateAsync(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TransactionUpdateRequest request)
        {
            var result = await _transactionService.UpdateAsync(id, request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _transactionService.DeleteAsync(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
