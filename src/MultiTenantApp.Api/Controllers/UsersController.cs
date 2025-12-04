using Microsoft.AspNetCore.Mvc;
using MultiTenantApp.Api.Attributes;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;

namespace MultiTenantApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
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
        [InvalidateCache("action:Users:*")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto model)
        {
            try
            {
                var user = await _userService.CreateUserAsync(model);
                return Ok(user);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
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
