using System.Threading.Tasks;
using MultiTenantApp.Domain.Entities;

namespace MultiTenantApp.Application.Interfaces
{
    public interface ITenantValidationService
    {
        Task<Tenant> ValidateAndGetTenantAsync(string identifier);
    }
}
