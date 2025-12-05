using MultiTenantApp.Web.Models.DTOs;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MultiTenantApp.Web.Interfaces;

namespace MultiTenantApp.Web.Services
{
    public class UserService : IUserService
    {
        private readonly AuthenticatedHttpClient _httpClient;

        public UserService(AuthenticatedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<UserDto>> GetUsers()
        {
            return await _httpClient.GetFromJsonAsync<List<UserDto>>("api/Users");
        }

        public async Task<PagedResponse<UserDto>> GetUsersPaged(PagedRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Users/list", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new System.Exception(error);
            }

            return await response.Content.ReadFromJsonAsync<PagedResponse<UserDto>>();
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
