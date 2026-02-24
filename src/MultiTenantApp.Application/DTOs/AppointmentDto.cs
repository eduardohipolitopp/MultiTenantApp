using MultiTenantApp.Domain.Enums;

namespace MultiTenantApp.Application.DTOs
{
    public class AppointmentDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public Guid VaccineId { get; set; }
        public string? VaccineName { get; set; }
        public string ProfessionalId { get; set; } = string.Empty;
        public string? ProfessionalName { get; set; }
        public DateTime ScheduledDateTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public AppointmentType Type { get; set; }
    }

    public class CreateAppointmentDto
    {
        public Guid PatientId { get; set; }
        public Guid VaccineId { get; set; }
        public string ProfessionalId { get; set; } = string.Empty;
        public DateTime ScheduledDateTime { get; set; }
        public AppointmentType Type { get; set; } = AppointmentType.Clinic;
    }

    public class UpdateAppointmentDto
    {
        public DateTime ScheduledDateTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public AppointmentType Type { get; set; }
    }

    public class AppointmentListDto
    {
        public Guid Id { get; set; }
        public string? PatientName { get; set; }
        public string? VaccineName { get; set; }
        public DateTime ScheduledDateTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public AppointmentType Type { get; set; }
    }
}
