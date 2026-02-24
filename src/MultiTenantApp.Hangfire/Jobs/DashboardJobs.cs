using Hangfire;
using MultiTenantApp.Application.Interfaces;

namespace MultiTenantApp.Hangfire.Jobs
{
    public class DashboardJobs
    {
        private readonly IDashboardService _dashboardService;

        public DashboardJobs(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task RunDailySnapshot()
        {
            await _dashboardService.GenerateDailySnapshot();
        }
    }
}
