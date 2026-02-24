using System.ComponentModel.DataAnnotations;

namespace MultiTenantApp.Web.Models.DTOs
{
    public class ClinicSettingsDto
    {
        [Required]
        public string ClinicName { get; set; } = string.Empty;
        
        [Range(0, 100)]
        public decimal CommissionPercentage { get; set; }
        
        public decimal HomeVisitBonus { get; set; }
        
        public int ReminderDaysBefore { get; set; }
        
        public int ExpirationAlertDays { get; set; }
        
        public string DefaultCurrency { get; set; } = "BRL";
    }
}
