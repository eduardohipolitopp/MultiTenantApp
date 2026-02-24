using MultiTenantApp.Domain.Enums;
using System;

namespace MultiTenantApp.Application.DTOs
{
    public class VaccineApplicationDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public Guid VaccineBatchId { get; set; }
        public string? BatchNumber { get; set; }
        public string? VaccineName { get; set; }
        public DateTime ApplicationDate { get; set; }
        public int DoseNumber { get; set; }
        public string ProfessionalId { get; set; } = string.Empty;
        public string? ProfessionalName { get; set; }
        public ApplicationType ApplicationType { get; set; }
        public decimal PaidAmount { get; set; }
    }

    public class CreateVaccineApplicationDto
    {
        public Guid PatientId { get; set; }
        public Guid VaccineId { get; set; } // Will find batch via FIFO
        public int DoseNumber { get; set; }
        public string ProfessionalId { get; set; } = string.Empty;
        public ApplicationType ApplicationType { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
    }

    public class VaccineApplicationListDto
    {
        public Guid Id { get; set; }
        public string? PatientName { get; set; }
        public string? VaccineName { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime ApplicationDate { get; set; }
        public int DoseNumber { get; set; }
        public string? ProfessionalName { get; set; }
    }
}
