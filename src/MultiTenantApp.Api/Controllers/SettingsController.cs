using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using System.Threading.Tasks;

namespace MultiTenantApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly IClinicSettingsService _settingsService;

        public SettingsController(IClinicSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var settings = await _settingsService.GetSettingsAsync();
            return Ok(settings);
        }

        [Authorize(Policy = "Settings.Edit")]
        [HttpPut]
        public async Task<IActionResult> Update(ClinicSettingsDto model)
        {
            await _settingsService.UpdateSettingsAsync(model);
            return NoContent();
        }
    }
}
