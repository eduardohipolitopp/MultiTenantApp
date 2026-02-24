using MultiTenantApp.Web.Models.DTOs;

namespace MultiTenantApp.Web.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetSnapshot();
    }
}
