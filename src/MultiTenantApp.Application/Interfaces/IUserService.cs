using System.Collections.Generic;
using System.Threading.Tasks;
using MultiTenantApp.Application.DTOs;

namespace MultiTenantApp.Application.Interfaces
{
    public interface IUserService
    {
        Task<PagedResponse<UserDto>> GetUsersPagedAsync(PagedRequest request);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto> CreateUserAsync(CreateUserDto model);
        Task UpdateUserAsync(string id, UpdateUserDto model);
        Task DeleteUserAsync(string id);
        Task<UserDto> GetUserByIdAsync(string id);
    }
}
