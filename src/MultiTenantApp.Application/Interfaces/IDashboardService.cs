using MultiTenantApp.Application.DTOs;

namespace MultiTenantApp.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardSnapshot();
        Task GenerateDailySnapshot();
    }
}
