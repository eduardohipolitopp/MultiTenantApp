using System.ComponentModel.DataAnnotations;
using MultiTenantApp.Domain.Attributes;
using MultiTenantApp.Domain.Common;

namespace MultiTenantApp.Domain.Entities
{
    [LogicalDelete]
    public class Vaccine : BaseTenantEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Manufacturer { get; set; }

        public int ApplicationAgeMonths { get; set; }

        public int Doses { get; set; }

        public int DoseIntervalDays { get; set; }

        public bool RequiresBooster { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
