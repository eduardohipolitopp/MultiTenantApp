using System.Threading.Tasks;
using MultiTenantApp.Application.DTOs;

namespace MultiTenantApp.Web.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> Login(LoginDto loginModel);
        Task Logout();
        Task RegisterAsync(RegisterDto registerModel);
        Task<IEnumerable<string>> GetPermissionsAsync();
    }
}
