using MultiTenantApp.Web.Models.DTOs;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MultiTenantApp.Web.Interfaces;

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

        public async Task<RuleDto> GetRuleById(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<RuleDto>($"api/Rules/{id}");
        }

        public async Task<RuleDto> CreateRule(CreateRuleDto model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Rules", model);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RuleDto>();
        }

        public async Task UpdateRule(Guid id, UpdateRuleDto model)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Rules/{id}", model);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteRule(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/Rules/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<UserRuleDto>> GetUserRules(string userId)
        {
            return await _httpClient.GetFromJsonAsync<List<UserRuleDto>>($"api/Rules/user/{userId}");
        }

        public async Task<UserRuleDto> AssignRuleToUser(AssignRuleDto model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Rules/assign", model);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserRuleDto>();
        }

        public async Task RemoveRuleFromUser(string userId, Guid ruleId)
        {
            var response = await _httpClient.DeleteAsync($"api/Rules/user/{userId}/rule/{ruleId}");
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateUserRulePermission(Guid userRuleId, int permissionType)
        {
            var response = await _httpClient.PutAsync($"api/Rules/userrule/{userRuleId}/permission/{permissionType}", null);
            response.EnsureSuccessStatusCode();
        }
    }
}
