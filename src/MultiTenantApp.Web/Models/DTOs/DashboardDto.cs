namespace MultiTenantApp.Web.Models.DTOs
{
    public class DashboardDto
    {
        public DateTime Date { get; set; }
        public int ApplicationsToday { get; set; }
        public decimal RevenueToday { get; set; }
        public int OverdueDoses { get; set; }
        public int ExpiringBatches { get; set; }
        public int ScheduledToday { get; set; }
        public int HomeVisitsMonth { get; set; }
        
        public List<MonthlyRevenueDto> Last6MonthsRevenue { get; set; } = new();
    }

    public class MonthlyRevenueDto
    {
        public string MonthName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }
}
