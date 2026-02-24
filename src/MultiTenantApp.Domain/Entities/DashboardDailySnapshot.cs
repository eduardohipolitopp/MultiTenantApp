using MultiTenantApp.Domain.Common;

namespace MultiTenantApp.Domain.Entities
{
    public class DashboardDailySnapshot : BaseTenantEntity
    {
        public DateTime Date { get; set; }
        public int ApplicationsToday { get; set; }
        public decimal RevenueToday { get; set; }
        public int OverdueDoses { get; set; }
        public int ExpiringBatches { get; set; }
        public int ScheduledToday { get; set; }
        public int HomeVisitsMonth { get; set; }
    }
}
