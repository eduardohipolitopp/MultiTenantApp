using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;

namespace MultiTenantApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("snapshot")]
        public async Task<ActionResult<DashboardDto>> GetSnapshot()
        {
            var snapshot = await _dashboardService.GetDashboardSnapshot();
            return Ok(snapshot);
        }

        [HttpPost("generate")]
        [Authorize(Roles = "Admin")] // Only admins can trigger manual generation
        public async Task<IActionResult> GenerateSnapshot()
        {
            await _dashboardService.GenerateDailySnapshot();
            return Ok(new { message = "Snapshot generation triggered for all tenants" });
        }
    }
}
