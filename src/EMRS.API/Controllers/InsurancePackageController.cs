using EMRS.Application.DTOs.InsurancePackageDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InsurancePackageController : ControllerBase
    {
        private readonly IInsurancePackageService _insurancePackageService;

        public InsurancePackageController(IInsurancePackageService insurancePackageService)
        {
            _insurancePackageService = insurancePackageService;
        }

        /// <summary>
        /// Create a new insurance package (Admin only)
        /// </summary>
        /// <param name="request">Insurance package details</param>
        /// <returns>Created insurance package</returns>
        //[Authorize(Roles = "ADMIN")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateInsurancePackage(
            [FromBody] InsurancePackageCreateRequest request)
        {
            var result = await _insurancePackageService.CreateInsurancePackage(request);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        /// <summary>
        /// Get all insurance packages (Public access for Renters to view)
        /// </summary>
        /// <returns>List of all insurance packages</returns>
        [AllowAnonymous]
        [HttpGet("")]
        public async Task<IActionResult> GetAllInsurancePackages()
        {
            var result = await _insurancePackageService.GetAllInsurancePackages();

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        /// <summary>
        /// Get insurance package by ID
        /// </summary>
        /// <param name="id">Insurance package ID</param>
        /// <returns>Insurance package details</returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInsurancePackageById(Guid id)
        {
            var result = await _insurancePackageService.GetInsurancePackageById(id);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
    }
}
