using Microsoft.AspNetCore.Mvc;
using MultiTenantApp.Api.Attributes;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Enums;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [RequirePermission("Users", PermissionType.View)]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITenantProvider _tenantProvider;

        public UsersController(IUserService userService, ITenantProvider tenantProvider)
        {
            _userService = userService;
            _tenantProvider = tenantProvider;
        }

        [HttpPost("list")]
        [Cached(durationMinutes: 10, varyByTenant: true)]
        public async Task<IActionResult> GetAll([FromBody] PagedRequest request)
        {
            var response = await _userService.GetUsersPagedAsync(request);
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost]
        [RequirePermission("Users", PermissionType.Edit)]
        [InvalidateCache("action:Users:*")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto model)
        {
            try
            {
                // Tenant validation is handled by the service layer
                // The service will use the TenantProvider to set the correct tenant
                var user = await _userService.CreateUserAsync(model);
                return Ok(user);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [RequirePermission("Users", PermissionType.Edit)]
        [InvalidateCache("action:Users:*")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto model)
        {
            try
            {
                await _userService.UpdateUserAsync(id, model);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [RequirePermission("Users", PermissionType.Edit)]
        [InvalidateCache("action:Users:*")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
