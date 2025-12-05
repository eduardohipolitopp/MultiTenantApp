using MultiTenantApp.Web.Models.DTOs;

namespace MultiTenantApp.Web.Interfaces
{
    public interface IRuleService
    {
        Task<List<RuleDto>> GetRules();
        Task<RuleDto> GetRuleById(Guid id);
        Task<RuleDto> CreateRule(CreateRuleDto model);
        Task UpdateRule(Guid id, UpdateRuleDto model);
        Task DeleteRule(Guid id);
        Task<List<UserRuleDto>> GetUserRules(string userId);
        Task<UserRuleDto> AssignRuleToUser(AssignRuleDto model);
        Task RemoveRuleFromUser(string userId, Guid ruleId);
        Task UpdateUserRulePermission(Guid userRuleId, int permissionType);
    }
}
