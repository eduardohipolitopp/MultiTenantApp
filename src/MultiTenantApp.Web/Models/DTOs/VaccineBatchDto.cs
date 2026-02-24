using System;

namespace MultiTenantApp.Web.Models.DTOs
{
    public class VaccineBatchDto
    {
        public Guid Id { get; set; }
        public Guid VaccineId { get; set; }
        public string? VaccineName { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime EntryDate { get; set; }
        public string? Supplier { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateVaccineBatchDto
    {
        public Guid VaccineId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string? Supplier { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateVaccineBatchDto : CreateVaccineBatchDto
    {
        public int AvailableQuantity { get; set; }
    }

    public class VaccineBatchListDto
    {
        public Guid Id { get; set; }
        public string VaccineName { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
