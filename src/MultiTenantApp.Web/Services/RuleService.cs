using MultiTenantApp.Application.DTOs;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MultiTenantApp.Web.Services
{
    public class RuleService : IRuleService
    {
        private readonly AuthenticatedHttpClient _httpClient;

        public RuleService(AuthenticatedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<RuleDto>> GetRules()
        {
            return await _httpClient.GetFromJsonAsync<List<RuleDto>>("api/Rules/list");
        }

        public async Task<PagedResponse<RuleDto>> GetRulesPaged(PagedRequest request)
        {
            var queryString = $"?Page={request.Page}&PageSize={request.PageSize}";
            
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                queryString += $"&SortBy={request.SortBy}&SortDescending={request.SortDescending}";
            }
            
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                queryString += $"&SearchTerm={Uri.EscapeDataString(request.SearchTerm)}";
            }

            return await _httpClient.GetFromJsonAsync<PagedResponse<RuleDto>>($"api/Rules/paged{queryString}");
        }

        public async Task CreateRule(string roleName)
        {
            await _httpClient.PostAsJsonAsync("api/Rules", roleName);
        }

        public async Task DeleteRule(string id)
        {
            await _httpClient.DeleteAsync($"api/Rules/{id}");
        }
    }
}
