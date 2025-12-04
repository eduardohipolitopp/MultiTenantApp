using System.Threading.Tasks;
using MultiTenantApp.Application.DTOs;

namespace MultiTenantApp.Application.Interfaces
{
    public interface IUserRegistrationService
    {
        Task RegisterAsync(RegisterDto model);
    }
}
