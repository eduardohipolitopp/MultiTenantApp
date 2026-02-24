using MultiTenantApp.Domain.Common;

namespace MultiTenantApp.Domain.Entities
{
    public class HomeVisitMonthlyClosing : BaseTenantEntity
    {
        public string ProfessionalId { get; set; } = string.Empty;
        public ApplicationUser? Professional { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalVisits { get; set; }
        public decimal BonusAmount { get; set; }
        public DateTime ClosedAt { get; set; } = DateTime.UtcNow;
    }
}
