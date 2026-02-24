using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MultiTenantApp.Web.Models.DTOs;
using MultiTenantApp.Web.Interfaces;

namespace MultiTenantApp.Web.Services
{
    public class VaccineBatchService : IVaccineBatchService
    {
        private readonly AuthenticatedHttpClient _httpClient;

        public VaccineBatchService(AuthenticatedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<VaccineBatchDto?> GetByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<VaccineBatchDto>($"api/VaccineBatches/details/{id}");
        }

        public async Task<VaccineBatchDto> CreateAsync(CreateVaccineBatchDto model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/VaccineBatches/create", model);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<VaccineBatchDto>();
        }

        public async Task UpdateAsync(Guid id, UpdateVaccineBatchDto model)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/VaccineBatches/update/{id}", model);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/VaccineBatches/delete/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<PagedResponse<VaccineBatchListDto>> GetAllAsync(PagedRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/VaccineBatches/list", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PagedResponse<VaccineBatchListDto>>();
        }

        public async Task<VaccineBatchDto?> GetNextAvailableBatchFIFO(Guid vaccineId)
        {
            return await _httpClient.GetFromJsonAsync<VaccineBatchDto>($"api/VaccineBatches/next-fifo/{vaccineId}");
        }
    }
}
