using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MultiTenantApp.Web.Services
{
    public class AuthenticatedHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenProvider _tokenProvider;

        public AuthenticatedHttpClient(HttpClient httpClient, ITokenProvider tokenProvider)
        {
            _httpClient = httpClient;
            _tokenProvider = tokenProvider;
        }

        public Uri? BaseAddress
        {
            get => _httpClient.BaseAddress;
            set => _httpClient.BaseAddress = value;
        }

        private void EnsureAuthorizationHeader()
        {
            var token = _tokenProvider?.Token;
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<T?> GetFromJsonAsync<T>(string requestUri, CancellationToken cancellationToken = default)
        {
            EnsureAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<T>(requestUri, cancellationToken);
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            EnsureAuthorizationHeader();
            return await _httpClient.GetAsync(requestUri, cancellationToken);
        }

        public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value, CancellationToken cancellationToken = default)
        {
            EnsureAuthorizationHeader();
            return await _httpClient.PostAsJsonAsync(requestUri, value, cancellationToken);
        }

        public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string requestUri, T value, CancellationToken cancellationToken = default)
        {
            EnsureAuthorizationHeader();
            return await _httpClient.PutAsJsonAsync(requestUri, value, cancellationToken);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            EnsureAuthorizationHeader();
            return await _httpClient.DeleteAsync(requestUri, cancellationToken);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            EnsureAuthorizationHeader();
            return await _httpClient.SendAsync(request, cancellationToken);
        }
    }
}
