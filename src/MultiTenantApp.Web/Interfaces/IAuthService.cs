using System.Threading.Tasks;
using MultiTenantApp.Web.Models.DTOs;

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
