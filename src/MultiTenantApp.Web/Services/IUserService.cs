using MultiTenantApp.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantApp.Web.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetUsers();
        Task<PagedResponse<UserDto>> GetUsersPaged(PagedRequest request);
        Task CreateUser(CreateUserDto user);
        Task UpdateUser(string id, UpdateUserDto user);
        Task DeleteUser(string id);
    }
}
