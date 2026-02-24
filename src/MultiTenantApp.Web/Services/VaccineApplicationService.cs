using MultiTenantApp.Web.Interfaces;
using MultiTenantApp.Web.Models.DTOs;
using System.Net.Http.Json;

namespace MultiTenantApp.Web.Services
{
    public class VaccineApplicationService : IVaccineApplicationService
    {
        private readonly HttpClient _httpClient;

        public VaccineApplicationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<VaccineApplicationDto?> GetByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<VaccineApplicationDto>($"api/vaccineapplications/{id}");
        }

        public async Task<PagedResponse<VaccineApplicationListDto>> GetAllAsync(PagedRequest request)
        {
            var queryString = $"?page={request.Page}&pageSize={request.PageSize}&searchTerm={request.SearchTerm}&sortBy={request.SortBy}&sortDescending={request.SortDescending}";
            return await _httpClient.GetFromJsonAsync<PagedResponse<VaccineApplicationListDto>>($"api/vaccineapplications{queryString}")
                   ?? new PagedResponse<VaccineApplicationListDto>(new List<VaccineApplicationListDto>(), 1, 10, 0);
        }

        public async Task<VaccineApplicationDto> ApplyVaccine(CreateVaccineApplicationDto model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/vaccineapplications/apply", model);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<VaccineApplicationDto>() ?? throw new Exception("Error applying vaccine");
        }
    }
}
