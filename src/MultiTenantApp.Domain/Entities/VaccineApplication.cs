using MultiTenantApp.Domain.Attributes;
using MultiTenantApp.Domain.Common;
using MultiTenantApp.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiTenantApp.Domain.Entities
{
    [LogicalDelete]
    public class VaccineApplication : BaseTenantEntity
    {
        [ForeignKey(nameof(Patient))]
        public Guid PatientId { get; set; }
        public Patient? Patient { get; set; }

        [ForeignKey(nameof(VaccineBatch))]
        public Guid VaccineBatchId { get; set; }
        public VaccineBatch? VaccineBatch { get; set; }

        public DateTime ApplicationDate { get; set; }
        public int DoseNumber { get; set; }

        [ForeignKey(nameof(Professional))]
        public string ProfessionalId { get; set; } = string.Empty;
        public ApplicationUser? Professional { get; set; }

        public ApplicationType ApplicationType { get; set; }
        public decimal PaidAmount { get; set; }
        public bool AlertSent { get; set; }
    }
}
