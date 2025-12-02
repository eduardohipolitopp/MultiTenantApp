using MultiTenantApp.Application.DTOs;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MultiTenantApp.Web.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<UserDto>> GetUsers()
        {
            return await _httpClient.GetFromJsonAsync<List<UserDto>>("api/Users");
        }

        public async Task<PagedResponse<UserDto>> GetUsersPaged(PagedRequest request)
        {
            var queryString = $"?Page={request.Page}&PageSize={request.PageSize}";
            
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                queryString += $"&SortBy={request.SortBy}&SortDescending={request.SortDescending}";
            }
            
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                queryString += $"&SearchTerm={Uri.EscapeDataString(request.SearchTerm)}";
            }

            return await _httpClient.GetFromJsonAsync<PagedResponse<UserDto>>($"api/Users/paged{queryString}");
        }

        public async Task CreateUser(CreateUserDto user)
        {
            await _httpClient.PostAsJsonAsync("api/Users", user);
        }

        public async Task UpdateUser(string id, UpdateUserDto user)
        {
            await _httpClient.PutAsJsonAsync($"api/Users/{id}", user);
        }

        public async Task DeleteUser(string id)
        {
            await _httpClient.DeleteAsync($"api/Users/{id}");
        }
    }
}
