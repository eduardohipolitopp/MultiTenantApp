using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Enums;
using MultiTenantApp.Infrastructure.Persistence;
using System.Text.Json;

namespace MultiTenantApp.Infrastructure.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private const int CacheExpirationMinutes = 30;

        public PermissionService(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<bool> HasPermissionAsync(string userId, string ruleName, PermissionType permissionType)
        {
            if(await HasAdminPermissionAsync(userId))
            {
                return true;
            }

            var cacheKey = $"permission:{userId}:{ruleName}";
            var cachedPermission = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedPermission))
            {
                var cachedType = (PermissionType)int.Parse(cachedPermission);
                return cachedType >= permissionType;
            }

            var userRule = await _context.UserRules
                .Include(ur => ur.Rule)
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.Rule!.Name == ruleName);

            if (userRule == null)
                return false;

            await _cache.SetStringAsync(
                cacheKey,
                ((int)userRule.PermissionType).ToString(),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes)
                });

            return userRule.PermissionType >= permissionType;
        }

        public async Task<bool> HasAdminPermissionAsync(string userId)
        {
            var cacheKey = $"permission:{userId}:Admin";
            var cachedPermission = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedPermission))
            {
                bool.TryParse(cachedPermission, out var cachedType);
                return cachedType;
            }

            var hasAdminRule = await _context.UserRules
                .Include(ur => ur.Rule)
                .AnyAsync(ur => ur.UserId == userId && ur.Rule!.Name == "Admin");

            await _cache.SetStringAsync(
                cacheKey,
                hasAdminRule.ToString(),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes)
                });

            return hasAdminRule;
        }

        public async Task<IEnumerable<UserRuleDto>> GetUserPermissionsAsync(string userId)
        {
            var userRules = await _context.UserRules
                .Include(ur => ur.Rule)
                .Where(ur => ur.UserId == userId)
                .Select(ur => new UserRuleDto
                {
                    Id = ur.Id,
                    UserId = ur.UserId,
                    RuleId = ur.RuleId,
                    RuleName = ur.Rule!.Name,
                    PermissionType = ur.PermissionType.ToString()
                })
                .ToListAsync();

            return userRules;
        }

        public async Task<UserRuleDto> AssignRuleToUserAsync(AssignRuleDto dto)
        {
            // Check if assignment already exists
            var existing = await _context.UserRules
                .FirstOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.RuleId == dto.RuleId);

            if (existing != null)
            {
                throw new InvalidOperationException("User already has this rule assigned");
            }

            var userRule = new UserRule
            {
                UserId = dto.UserId,
                RuleId = dto.RuleId,
                PermissionType = (PermissionType)dto.PermissionType
            };

            _context.UserRules.Add(userRule);
            await _context.SaveChangesAsync();

            // Invalidate cache
            var rule = await _context.Rules.FindAsync(dto.RuleId);
            if (rule != null)
            {
                await _cache.RemoveAsync($"permission:{dto.UserId}:{rule.Name}");
            }

            return new UserRuleDto
            {
                Id = userRule.Id,
                UserId = userRule.UserId,
                RuleId = userRule.RuleId,
                RuleName = rule?.Name ?? string.Empty,
                PermissionType = userRule.PermissionType.ToString()
            };
        }

        public async Task RemoveRuleFromUserAsync(string userId, Guid ruleId)
        {
            var userRule = await _context.UserRules
                .Include(ur => ur.Rule)
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RuleId == ruleId);

            if (userRule == null)
            {
                throw new KeyNotFoundException("User rule not found");
            }

            _context.UserRules.Remove(userRule);
            await _context.SaveChangesAsync();

            // Invalidate cache
            await _cache.RemoveAsync($"permission:{userId}:{userRule.Rule!.Name}");
        }

        public async Task UpdateUserRulePermissionAsync(Guid userRuleId, PermissionType permissionType)
        {
            var userRule = await _context.UserRules
                .Include(ur => ur.Rule)
                .FirstOrDefaultAsync(ur => ur.Id == userRuleId);

            if (userRule == null)
            {
                throw new KeyNotFoundException("User rule not found");
            }

            userRule.PermissionType = permissionType;
            await _context.SaveChangesAsync();

            // Invalidate cache
            await _cache.RemoveAsync($"permission:{userRule.UserId}:{userRule.Rule!.Name}");
        }

        public async Task<IEnumerable<UserRuleDto>> GetUserRulesByUserIdAsync(string userId)
        {
            return await GetUserPermissionsAsync(userId);
        }

        public async Task<IEnumerable<string>> GetAccessibleRulesAsync(string userId)
        {
            if (await HasAdminPermissionAsync(userId))
            {
                return await _context.Rules.Select(r => r.Name).ToListAsync();
            }

            return await _context.UserRules
                .Include(ur => ur.Rule)
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Rule!.Name)
                .ToListAsync();
        }
    }
}
