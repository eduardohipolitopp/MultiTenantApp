using System;

namespace MultiTenantApp.Application.DTOs
{
    public class ClinicSettingsDto
    {
        public string ClinicName { get; set; } = string.Empty;
        public decimal CommissionPercentage { get; set; }
        public decimal HomeVisitBonus { get; set; }
        public int ReminderDaysBefore { get; set; }
        public int ExpirationAlertDays { get; set; }
        public string DefaultCurrency { get; set; } = "BRL";
    }
}
