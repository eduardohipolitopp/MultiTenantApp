using MultiTenantApp.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantApp.Web.Services
{
    public interface IRuleService
    {
        Task<List<RuleDto>> GetRules();
        Task<PagedResponse<RuleDto>> GetRulesPaged(PagedRequest request);
        Task CreateRule(string roleName);
        Task DeleteRule(string id);
    }
}
