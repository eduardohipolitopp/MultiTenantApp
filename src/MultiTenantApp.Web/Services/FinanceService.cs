using MultiTenantApp.Web.Interfaces;
using MultiTenantApp.Web.Models.DTOs;
using System.Net.Http.Json;

namespace MultiTenantApp.Web.Services
{
    public class FinanceService : IFinanceService
    {
        private readonly HttpClient _httpClient;

        public FinanceService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<FinanceDto?> GetByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<FinanceDto>($"api/finance/{id}");
        }

        public async Task<PagedResponse<FinanceListDto>> GetAllAsync(PagedRequest request)
        {
            var queryString = $"?page={request.Page}&pageSize={request.PageSize}&searchTerm={request.SearchTerm}&sortBy={request.SortBy}&sortDescending={request.SortDescending}";
            return await _httpClient.GetFromJsonAsync<PagedResponse<FinanceListDto>>($"api/finance{queryString}")
                   ?? new PagedResponse<FinanceListDto>(new List<FinanceListDto>(), 1, 10, 0);
        }

        public async Task<FinanceDto> RegisterPayment(CreateFinanceDto model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/finance/register", model);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<FinanceDto>() ?? throw new Exception("Error registering payment");
        }

        public async Task<FinanceSummaryDto> GetSummary(DateTime? start = null, DateTime? end = null)
        {
            var queryString = "";
            if (start.HasValue || end.HasValue)
            {
                var parts = new List<string>();
                if (start.HasValue) parts.Add($"start={start.Value:O}");
                if (end.HasValue) parts.Add($"end={end.Value:O}");
                queryString = "?" + string.Join("&", parts);
            }
            return await _httpClient.GetFromJsonAsync<FinanceSummaryDto>($"api/finance/summary{queryString}")
                   ?? new FinanceSummaryDto();
        }

        public async Task<bool> MonthlyClosing(int month, int year)
        {
            var response = await _httpClient.PostAsync($"api/finance/close?month={month}&year={year}", null);
            return response.IsSuccessStatusCode;
        }
    }
}
