using MultiTenantApp.Domain.Attributes;
using MultiTenantApp.Domain.Common;
using MultiTenantApp.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiTenantApp.Domain.Entities
{
    [LogicalDelete]
    public class Appointment : BaseTenantEntity
    {
        [ForeignKey(nameof(Patient))]
        public Guid PatientId { get; set; }
        public Patient? Patient { get; set; }

        [ForeignKey(nameof(Vaccine))]
        public Guid VaccineId { get; set; }
        public Vaccine? Vaccine { get; set; }

        [ForeignKey(nameof(Professional))]
        public string ProfessionalId { get; set; } = string.Empty;
        public ApplicationUser? Professional { get; set; }

        public DateTime ScheduledDateTime { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
        public AppointmentType Type { get; set; } = AppointmentType.Clinic;
    }
}
