using EMRS.Application.DTOs.InsuranceClaimDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InsuranceClaimController : ControllerBase
    {
        private readonly IInsuranceClaimService _insuranceClaimService;

        public InsuranceClaimController(IInsuranceClaimService insuranceClaimService)
        {
            _insuranceClaimService = insuranceClaimService;
        }

        // POST: api/insuranceclaim/create
        [Authorize(Roles = "RENTER")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateInsuranceClaim([FromForm] CreateInsuranceClaimRequest request)
        {
            var result = await _insuranceClaimService.CreateInsuranceClaim(request);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // GET: api/insuranceclaim/my-claims
        [Authorize(Roles = "RENTER")]
        [HttpGet("my-claims")]
        public async Task<IActionResult> GetMyInsuranceClaims()
        {
            var result = await _insuranceClaimService.GetMyInsuranceClaims();
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // GET: api/insuranceclaim/{id}
        [Authorize(Roles = "RENTER,MANAGER,ADMIN")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInsuranceClaimDetail(Guid id)
        {
            var result = await _insuranceClaimService.GetInsuranceClaimDetail(id);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // GET: api/insuranceclaim/manager/branch-claims
        [Authorize(Roles = "MANAGER")]
        [HttpGet("manager/branch-claims")]
        public async Task<IActionResult> GetBranchInsuranceClaims()
        {
            var result = await _insuranceClaimService.GetBranchInsuranceClaims();
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        // GET: api/insuranceclaim/manager/{id}
        [Authorize(Roles = "MANAGER")]
        [HttpGet("manager/{id}")]
        public async Task<IActionResult> GetInsuranceClaimForManager(Guid id)
        {
            var result = await _insuranceClaimService.GetInsuranceClaimForManager(id);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
    }
}
