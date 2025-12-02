using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MultiTenantApp.Application.DTOs;


namespace MultiTenantApp.Web.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ProductDto>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ProductDto>>("api/Products");
        }

        public async Task<ProductDto> GetByIdAsync(System.Guid id)
        {
            return await _httpClient.GetFromJsonAsync<ProductDto>($"api/Products/{id}");
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Products", model);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProductDto>();
        }

        public async Task DeleteAsync(System.Guid id)
        {
            await _httpClient.DeleteAsync($"api/Products/{id}");
        }

        public async Task<PagedResponse<ProductDto>> GetPagedAsync(PagedRequest request)
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

            return await _httpClient.GetFromJsonAsync<PagedResponse<ProductDto>>($"api/Products/paged{queryString}");
        }
    }
}
