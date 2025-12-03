using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Api.Attributes;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("list")]
        [Cached(durationMinutes: 10, varyByTenant: true)]
        public async Task<IActionResult> GetAll([FromBody] PagedRequest request)
        {
            System.Linq.Expressions.Expression<Func<ApplicationUser, bool>> filter = null;
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                filter = p => p.Email.Contains(request.SearchTerm) || p.UserName.Contains(request.SearchTerm);
            }

            var query = _userManager.Users.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync();
            var users = await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync();

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                TenantId = u.TenantId.ToString()
            }).ToList();

            var response = new PagedResponse<UserDto>(userDtos, request.Page, request.PageSize, totalCount);
            return Ok(response);
        }

        [HttpGet]
        public IActionResult Get()
        {
            // This will be filtered by Global Query Filter for Tenant
            var users = _userManager.Users.ToList();
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                TenantId = u.TenantId.ToString()
            }).ToList();

            return Ok(userDtos);
        }

        [HttpPost]
        [InvalidateCache("action:Users:*")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                TenantId = System.Guid.Parse(model.TenantId)
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }
                return Ok(user);
            }

            return BadRequest(result.Errors);
        }

        [HttpPut("{id}")]
        [InvalidateCache("action:Users:*")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.Email = model.Email;
            user.UserName = model.Email; // Keep them synced
            
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Update roles if needed
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        [HttpDelete("{id}")]
        [InvalidateCache("action:Users:*")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded) return Ok();

            return BadRequest(result.Errors);
        }
    }
}
