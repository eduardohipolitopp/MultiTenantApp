using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MultiTenantApp.Web.Models.DTOs;
using MultiTenantApp.Web.Interfaces;

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
            return await _httpClient.GetFromJsonAsync<ProductDto>($"api/Products/details/{id}");
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Products/create", model);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProductDto>();
        }

        public async Task UpdateAsync(System.Guid id, UpdateProductDto model)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Products/update/{id}", model);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(System.Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/Products/delete/{id}");
            response.EnsureSuccessStatusCode();
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
