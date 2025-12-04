using System.Threading.Tasks;
using MultiTenantApp.Application.DTOs;

namespace MultiTenantApp.Application.Interfaces
{
    public interface IAuthenticationService
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginDto model);
    }
}
