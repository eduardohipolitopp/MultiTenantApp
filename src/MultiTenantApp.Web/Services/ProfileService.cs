using System.Net.Http.Json;
using MultiTenantApp.Web.Models.DTOs;
using MultiTenantApp.Web.Interfaces;

namespace MultiTenantApp.Web.Services
{
    public class ProfileService : IProfileService
    {
        private readonly AuthenticatedHttpClient _httpClient;

        public ProfileService(AuthenticatedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserProfileDto?> GetMyProfileAsync()
        {
            return await _httpClient.GetFromJsonAsync<UserProfileDto>("api/Profile/me");
        }

        public async Task UpdateMyProfileAsync(UpdateProfileDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/Profile/me", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task<string> UploadAvatarAsync(Stream fileStream, string fileName)
        {
            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(fileStream);
            content.Add(streamContent, "file", fileName);

            var response = await _httpClient.PostAsync("api/Profile/me/avatar", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AvatarUploadResponse>();
            return result?.AvatarUrl ?? string.Empty;
        }

        public async Task ChangePasswordAsync(ChangePasswordDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Profile/me/change-password", dto);
            response.EnsureSuccessStatusCode();
        }

        private class AvatarUploadResponse
        {
            public string AvatarUrl { get; set; } = string.Empty;
        }
    }
}
