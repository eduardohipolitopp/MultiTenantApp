using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Infrastructure.Persistence;

namespace MultiTenantApp.Infrastructure.Services
{
    public class RuleService : IRuleService
    {
        private readonly ApplicationDbContext _context;

        public RuleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RuleDto>> GetAllRulesAsync()
        {
            return await _context.Rules
                .Select(r => new RuleDto
                {
                    Id = r.Id.ToString(),
                    Name = r.Name,
                    Description = r.Description
                })
                .ToListAsync();
        }

        public async Task<RuleDto?> GetRuleByIdAsync(Guid id)
        {
            var rule = await _context.Rules.FindAsync(id);
            
            if (rule == null)
                return null;

            return new RuleDto
            {
                Id = rule.Id.ToString(),
                Name = rule.Name,
                Description = rule.Description
            };
        }

        public async Task<RuleDto> CreateRuleAsync(CreateRuleDto dto)
        {
            // Check if rule with same name already exists
            var exists = await _context.Rules.AnyAsync(r => r.Name == dto.Name);
            if (exists)
            {
                throw new InvalidOperationException($"Rule with name '{dto.Name}' already exists");
            }

            var rule = new Rule
            {
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Rules.Add(rule);
            await _context.SaveChangesAsync();

            return new RuleDto
            {
                Id = rule.Id.ToString(),
                Name = rule.Name,
                Description = rule.Description
            };
        }

        public async Task UpdateRuleAsync(Guid id, UpdateRuleDto dto)
        {
            var rule = await _context.Rules.FindAsync(id);
            
            if (rule == null)
            {
                throw new KeyNotFoundException("Rule not found");
            }

            // Check if another rule with same name exists
            var exists = await _context.Rules.AnyAsync(r => r.Name == dto.Name && r.Id != id);
            if (exists)
            {
                throw new InvalidOperationException($"Rule with name '{dto.Name}' already exists");
            }

            rule.Name = dto.Name;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRuleAsync(Guid id)
        {
            var rule = await _context.Rules.FindAsync(id);
            
            if (rule == null)
            {
                throw new KeyNotFoundException("Rule not found");
            }

            // Check if rule is assigned to any users
            var hasUsers = await _context.UserRules.AnyAsync(ur => ur.RuleId == id);
            if (hasUsers)
            {
                throw new InvalidOperationException("Cannot delete rule that is assigned to users");
            }

            _context.Rules.Remove(rule);
            await _context.SaveChangesAsync();
        }
    }
}
