using MultiTenantApp.Web.Models.DTOs;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MultiTenantApp.Web.Services
{
    public interface ISettingsService
    {
        Task<ClinicSettingsDto> GetSettings();
        Task UpdateSettings(ClinicSettingsDto settings);
    }

    public class SettingsService : ISettingsService
    {
        private readonly AuthenticatedHttpClient _httpClient;

        public SettingsService(AuthenticatedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ClinicSettingsDto> GetSettings()
        {
            return await _httpClient.GetFromJsonAsync<ClinicSettingsDto>("api/Settings");
        }

        public async Task UpdateSettings(ClinicSettingsDto settings)
        {
            var response = await _httpClient.PutAsJsonAsync("api/Settings", settings);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new System.Exception(error);
            }
        }
    }
}
