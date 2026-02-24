using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MultiTenantApp.Domain.Attributes;
using MultiTenantApp.Domain.Common;

namespace MultiTenantApp.Domain.Entities
{
    [LogicalDelete]
    public class VaccineBatch : BaseTenantEntity
    {
        [Required]
        public Guid VaccineId { get; set; }

        [ForeignKey(nameof(VaccineId))]
        public Vaccine? Vaccine { get; set; }

        [Required]
        [MaxLength(50)]
        public string BatchNumber { get; set; } = string.Empty;

        public int TotalQuantity { get; set; }

        public int AvailableQuantity { get; set; }

        [Required]
        public DateTime ExpirationDate { get; set; }

        public DateTime EntryDate { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? Supplier { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
