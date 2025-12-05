using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Domain.Enums;

namespace MultiTenantApp.Application.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(string userId, string ruleName, PermissionType permissionType);
        Task<IEnumerable<UserRuleDto>> GetUserPermissionsAsync(string userId);
        Task<UserRuleDto> AssignRuleToUserAsync(AssignRuleDto dto);
        Task RemoveRuleFromUserAsync(string userId, Guid ruleId);
        Task UpdateUserRulePermissionAsync(Guid userRuleId, PermissionType permissionType);
        Task<IEnumerable<UserRuleDto>> GetUserRulesByUserIdAsync(string userId);
    }
}
