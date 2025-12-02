using System.Threading.Tasks;
using MultiTenantApp.Application.DTOs;

namespace MultiTenantApp.Web.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> Login(LoginDto loginModel);
        Task Logout();
        Task RegisterAsync(RegisterDto registerModel);
    }
}
