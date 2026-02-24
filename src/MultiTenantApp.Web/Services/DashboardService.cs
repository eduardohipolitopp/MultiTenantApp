using MultiTenantApp.Web.Interfaces;
using MultiTenantApp.Web.Models.DTOs;
using System.Net.Http.Json;

namespace MultiTenantApp.Web.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly HttpClient _httpClient;

        public DashboardService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DashboardDto> GetSnapshot()
        {
            return await _httpClient.GetFromJsonAsync<DashboardDto>("api/dashboard/snapshot") 
                   ?? new DashboardDto();
        }
    }
}
