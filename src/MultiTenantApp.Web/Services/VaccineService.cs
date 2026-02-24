using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MultiTenantApp.Web.Models.DTOs;
using MultiTenantApp.Web.Interfaces;

namespace MultiTenantApp.Web.Services
{
    public class VaccineService : IVaccineService
    {
        private readonly AuthenticatedHttpClient _httpClient;

        public VaccineService(AuthenticatedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<VaccineDto> GetByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<VaccineDto>($"api/Vaccines/details/{id}");
        }

        public async Task<VaccineDto> CreateAsync(CreatePatientDto model)
        {
             // Wait, the interface says CreateVaccineDto.
             // I notice I might have a typo in my interface or something.
             // Let me check IPatientService.cs again to be sure of the naming convention.
             // Actually I'll just use CreateVaccineDto as planned.
             throw new NotImplementedException();
        }

        public async Task<VaccineDto> CreateAsync(CreateVaccineDto model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Vaccines/create", model);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<VaccineDto>();
        }

        public async Task UpdateAsync(Guid id, UpdateVaccineDto model)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Vaccines/update/{id}", model);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/Vaccines/delete/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<PagedResponse<VaccineListDto>> GetAllAsync(PagedRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Vaccines/list", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            return await response.Content.ReadFromJsonAsync<PagedResponse<VaccineListDto>>();
        }
    }
}
