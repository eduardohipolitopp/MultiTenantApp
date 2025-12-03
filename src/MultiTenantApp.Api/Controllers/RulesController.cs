using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MultiTenantApp.Api.Attributes;
using MultiTenantApp.Application.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public class RulesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RulesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpGet("list")]
        public IActionResult Get()
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles);
        }

        [HttpPost]
        [InvalidateCache("action:Rules:*")]
        public async Task<IActionResult> Create([FromBody] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest("Role name is required");

            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
                return Ok(role);

            return BadRequest(result.Errors);
        }

        [HttpPut("{id}")]
        [InvalidateCache("action:Rules:*")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateRuleDto model)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            role.Name = model.Name;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded) return Ok();

            return BadRequest(result.Errors);
        }

        [HttpDelete("{id}")]
        [InvalidateCache("action:Rules:*")]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
                return Ok();

            return BadRequest(result.Errors);
        }
    }
}
