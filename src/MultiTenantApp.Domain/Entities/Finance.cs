using MultiTenantApp.Domain.Attributes;
using MultiTenantApp.Domain.Common;
using MultiTenantApp.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiTenantApp.Domain.Entities
{
    [LogicalDelete]
    public class Finance : BaseTenantEntity
    {
        [ForeignKey(nameof(Patient))]
        public Guid PatientId { get; set; }
        public Patient? Patient { get; set; }

        [ForeignKey(nameof(Professional))]
        public string ProfessionalId { get; set; } = string.Empty;
        public ApplicationUser? Professional { get; set; }

        [ForeignKey(nameof(Vaccine))]
        public Guid? VaccineId { get; set; }
        public Vaccine? Vaccine { get; set; }

        public decimal Amount { get; set; }
        public FinanceType Type { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal CommissionCalculated { get; set; }
    }
}
