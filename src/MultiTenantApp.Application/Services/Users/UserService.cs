using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Entities;

namespace MultiTenantApp.Application.Services.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<PagedResponse<UserDto>> GetUsersPagedAsync(PagedRequest request)
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

            return new PagedResponse<UserDto>(userDtos, request.Page, request.PageSize, totalCount);
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users
                .Include(u => u.UserRules)
                    .ThenInclude(ur => ur.Rule)
                .ToListAsync();
                
            return users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                TenantId = u.TenantId.ToString(),
                Rules = u.UserRules.Select(ur => new UserRuleDto
                {
                    Id = ur.Id,
                    UserId = ur.UserId,
                    RuleId = ur.RuleId,
                    RuleName = ur.Rule?.Name ?? string.Empty,
                    PermissionType = ur.PermissionType.ToString()
                }).ToList()
            }).ToList();
        }

        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            var user = await _userManager.Users
                .Include(u => u.UserRules)
                    .ThenInclude(ur => ur.Rule)
                .FirstOrDefaultAsync(u => u.Id == id);
                
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                TenantId = user.TenantId.ToString(),
                Rules = user.UserRules.Select(ur => new UserRuleDto
                {
                    Id = ur.Id,
                    UserId = ur.UserId,
                    RuleId = ur.RuleId,
                    RuleName = ur.Rule?.Name ?? string.Empty,
                    PermissionType = ur.PermissionType.ToString()
                }).ToList()
            };
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                TenantId = Guid.Parse(model.TenantId)
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (!string.IsNullOrEmpty(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                TenantId = user.TenantId.ToString()
            };
        }

        public async Task UpdateUserAsync(string id, UpdateUserDto model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) throw new KeyNotFoundException($"User with ID {id} not found.");

            user.Email = model.Email;
            user.UserName = model.Email;
            
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        public async Task DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) throw new KeyNotFoundException($"User with ID {id} not found.");

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
