using MultiTenantApp.Domain.Common;

namespace MultiTenantApp.Domain.Entities
{
    public class ClinicSettings : BaseTenantEntity
    {
        public string ClinicName { get; set; } = string.Empty;
        public decimal CommissionPercentage { get; set; }
        public decimal HomeVisitBonus { get; set; }
        public int ReminderDaysBefore { get; set; } = 3;
        public int ExpirationAlertDays { get; set; } = 30;
        public string DefaultCurrency { get; set; } = "BRL";
    }
}
