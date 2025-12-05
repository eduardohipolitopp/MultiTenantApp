using MultiTenantApp.Application.DTOs;

namespace MultiTenantApp.Application.Interfaces
{
    public interface IRuleService
    {
        Task<IEnumerable<RuleDto>> GetAllRulesAsync();
        Task<RuleDto?> GetRuleByIdAsync(Guid id);
        Task<RuleDto> CreateRuleAsync(CreateRuleDto dto);
        Task UpdateRuleAsync(Guid id, UpdateRuleDto dto);
        Task DeleteRuleAsync(Guid id);
    }
}
