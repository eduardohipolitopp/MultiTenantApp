using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MultiTenantApp.Web.Models.DTOs;
using MultiTenantApp.Web.Interfaces;

namespace MultiTenantApp.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthenticatedHttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILocalStorageService _localStorage;
        private readonly ITokenProvider _tokenProvider;

        public AuthService(AuthenticatedHttpClient httpClient,
                           AuthenticationStateProvider authenticationStateProvider,
                           ILocalStorageService localStorage,
                           ITokenProvider tokenProvider)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
            _tokenProvider = tokenProvider;
        }

        public async Task<LoginResponseDto> Login(LoginDto loginModel)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", loginModel);

            // Read raw content once so we can both inspect and deserialize
            var rawContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // If API returned a structured error JSON, keep it; otherwise wrap in a friendly message
                var message = string.IsNullOrWhiteSpace(rawContent)
                    ? $"Login failed with status code {(int)response.StatusCode} ({response.StatusCode})."
                    : rawContent;

                throw new System.Exception(message);
            }

            if (string.IsNullOrWhiteSpace(rawContent))
            {
                throw new System.Exception("Login API returned success but with an empty response body. Expected a JSON with token information.");
            }

            LoginResponseDto? loginResult;
            try
            {
                loginResult = System.Text.Json.JsonSerializer.Deserialize<LoginResponseDto>(
                    rawContent,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new System.Exception($"Failed to parse login response as JSON. Raw response: {rawContent}", ex);
            }

            if (loginResult == null || string.IsNullOrWhiteSpace(loginResult.Token))
            {
                throw new System.Exception($"Login API returned an invalid response. Raw response: {rawContent}");
            }

            await _tokenProvider.SetTokenAsync(loginResult.Token);
            await _localStorage.SetItemAsync("authToken", loginResult.Token);
            await _localStorage.SetItemAsync("tenantId", loginModel.TenantId); // Store tenant for future requests if needed

            // Fetch and store permissions
            try 
            {
                var permissions = await GetPermissionsAsync();
                await _localStorage.SetItemAsync("userPermissions", permissions);
            }
            catch
            {
                // If fetching permissions fails, we might want to log it or handle it, 
                // but for now we proceed with login. The user will just have no extra permissions.
            }

            ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(loginResult.Token);

            return loginResult;
        }

        public async Task Logout()
        {
            await _tokenProvider.ClearTokenAsync();
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("tenantId");
            await _localStorage.RemoveItemAsync("userPermissions");
            ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
        }

        public async Task RegisterAsync(RegisterDto registerModel)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/register", registerModel);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new System.Exception(error);
            }
        }

        public async Task<IEnumerable<string>> GetPermissionsAsync()
        {
            var response = await _httpClient.GetAsync("api/Profile/permissions");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<string>>() ?? new List<string>();
            }
            return new List<string>();
        }
    }
}
