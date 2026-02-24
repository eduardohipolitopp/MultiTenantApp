using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MultiTenantApp.Web.Models.DTOs;
using MultiTenantApp.Web.Interfaces;

namespace MultiTenantApp.Web.Services
{
    public class PatientService : IPatientService
    {
        private readonly AuthenticatedHttpClient _httpClient;

        public PatientService(AuthenticatedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PatientDto> GetByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<PatientDto>($"api/Patients/details/{id}");
        }

        public async Task<PatientDto> CreateAsync(CreatePatientDto model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Patients/create", model);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PatientDto>();
        }

        public async Task UpdateAsync(Guid id, UpdatePatientDto model)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Patients/update/{id}", model);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/Patients/delete/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<PagedResponse<PatientListDto>> GetAllAsync(PagedRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Patients/list", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            return await response.Content.ReadFromJsonAsync<PagedResponse<PatientListDto>>();
        }
    }
}
