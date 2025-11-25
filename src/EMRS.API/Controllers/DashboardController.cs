using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IBranchService _branchService;
        private readonly IVehicleService _vehicleService;
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;

        public DashboardController(ITransactionService transactionService,IAccountService accountService, IVehicleService vehicleService,
            IBranchService branchService)
        {
            _accountService = accountService;
            _vehicleService = vehicleService;
            _branchService = branchService;
            _transactionService = transactionService;
        }

        [HttpGet("admin/vehicle/model")]
        public async Task<IActionResult> GetVehicleModelDashboard()
        {
            var result = await _vehicleService.GetDashboardInfomationForVehicleModel();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("admin/vehicle")]
        public async Task<IActionResult> GetVehicleDashboard()
        {
            var result = await _vehicleService.GetDashboardInfomationForVehicle();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("admin/branch")]
        public async Task<IActionResult> GetBranchDashboard()
        {
            var result = await _branchService.GetDashboardInformationForBranches();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("admin/accounts")]
        public async Task<IActionResult> GetAccountDashboard()
        {
            var result = await _accountService.GetDashboardInformationForAccounts();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("admin/transactions")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            var result = await _transactionService.GetTotalRevenueAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
