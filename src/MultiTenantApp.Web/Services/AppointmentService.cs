using MultiTenantApp.Web.Interfaces;
using MultiTenantApp.Web.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MultiTenantApp.Web.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly HttpClient _httpClient;

        public AppointmentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AppointmentDto?> GetByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<AppointmentDto>($"api/appointments/{id}");
        }

        public async Task<PagedResponse<AppointmentListDto>> GetAllAsync(PagedRequest request)
        {
            var queryString = $"?page={request.Page}&pageSize={request.PageSize}&searchTerm={request.SearchTerm}&sortBy={request.SortBy}&sortDescending={request.SortDescending}";
            return await _httpClient.GetFromJsonAsync<PagedResponse<AppointmentListDto>>($"api/appointments{queryString}") 
                   ?? new PagedResponse<AppointmentListDto>(new List<AppointmentListDto>(), 1, 10, 0);
        }

        public async Task<IEnumerable<AppointmentDto>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<AppointmentDto>>($"api/appointments/calendar?start={start:O}&end={end:O}")
                   ?? new List<AppointmentDto>();
        }

        public async Task<AppointmentDto> CreateAsync(CreateAppointmentDto model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/appointments", model);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AppointmentDto>() ?? throw new Exception("Error creating appointment");
        }

        public async Task UpdateAsync(Guid id, UpdateAppointmentDto model)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/appointments/{id}", model);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/appointments/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
