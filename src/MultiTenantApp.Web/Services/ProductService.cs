using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MultiTenantApp.Application.DTOs;


namespace MultiTenantApp.Web.Services
{
    public class ProductService : IProductService
    {
        private readonly AuthenticatedHttpClient _httpClient;

        public ProductService(AuthenticatedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProductDto> GetByIdAsync(System.Guid id)
        {
            return await _httpClient.GetFromJsonAsync<ProductDto>($"api/Products/{id}");
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Products/create", model);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProductDto>();
        }

        public async Task DeleteAsync(System.Guid id)
        {
            await _httpClient.DeleteAsync($"api/Products/{id}");
        }

        public async Task<PagedResponse<ProductDto>> GetPagedAsync(PagedRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Products/list", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new System.Exception(error);
            }

            return await response.Content.ReadFromJsonAsync<PagedResponse<ProductDto>>();
        }
    }
}
