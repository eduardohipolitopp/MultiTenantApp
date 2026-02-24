using MultiTenantApp.Domain.Common;

namespace MultiTenantApp.Domain.Entities
{
    public class CommissionMonthlyClosing : BaseTenantEntity
    {
        public string ProfessionalId { get; set; } = string.Empty;
        public ApplicationUser? Professional { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalApplications { get; set; }
        public int TotalHomeVisits { get; set; }
        public decimal CommissionAmount { get; set; }
        public DateTime ClosedAt { get; set; } = DateTime.UtcNow;
    }
}
