using Microsoft.AspNetCore.Mvc;
using MultiTenantApp.Api.Attributes;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Enums;

namespace MultiTenantApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [RequirePermission("Rules", PermissionType.View)]
    public class RulesController : ControllerBase
    {
        private readonly IRuleService _ruleService;
        private readonly IPermissionService _permissionService;

        public RulesController(IRuleService ruleService, IPermissionService permissionService)
        {
            _ruleService = ruleService;
            _permissionService = permissionService;
        }

        [HttpGet("list")]
        public async Task<IActionResult> Get()
        {
            var rules = await _ruleService.GetAllRulesAsync();
            return Ok(rules);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var rule = await _ruleService.GetRuleByIdAsync(id);
            if (rule == null)
                return NotFound();
            return Ok(rule);
        }

        [HttpPost]
        [RequirePermission("Rules", PermissionType.Edit)]
        [InvalidateCache("action:Rules:*")]
        public async Task<IActionResult> Create([FromBody] CreateRuleDto model)
        {
            try
            {
                var rule = await _ruleService.CreateRuleAsync(model);
                return Ok(rule);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [RequirePermission("Rules", PermissionType.Edit)]
        [InvalidateCache("action:Rules:*")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRuleDto model)
        {
            try
            {
                await _ruleService.UpdateRuleAsync(id, model);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [RequirePermission("Rules", PermissionType.Edit)]
        [InvalidateCache("action:Rules:*")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _ruleService.DeleteRuleAsync(id);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // User-Rule assignment endpoints
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserRules(string userId)
        {
            var userRules = await _permissionService.GetUserRulesByUserIdAsync(userId);
            return Ok(userRules);
        }

        [HttpPost("assign")]
        [RequirePermission("Rules", PermissionType.Edit)]
        [InvalidateCache("action:Rules:*")]
        public async Task<IActionResult> AssignRule([FromBody] AssignRuleDto model)
        {
            try
            {
                var userRule = await _permissionService.AssignRuleToUserAsync(model);
                return Ok(userRule);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("user/{userId}/rule/{ruleId}")]
        [RequirePermission("Rules", PermissionType.Edit)]
        [InvalidateCache("action:Rules:*")]
        public async Task<IActionResult> RemoveRule(string userId, Guid ruleId)
        {
            try
            {
                await _permissionService.RemoveRuleFromUserAsync(userId, ruleId);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("userrule/{userRuleId}/permission/{permissionType}")]
        [RequirePermission("Rules", PermissionType.Edit)]
        [InvalidateCache("action:Rules:*")]
        public async Task<IActionResult> UpdatePermission(Guid userRuleId, int permissionType)
        {
            try
            {
                await _permissionService.UpdateUserRulePermissionAsync(userRuleId, (PermissionType)permissionType);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
