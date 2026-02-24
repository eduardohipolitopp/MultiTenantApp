using System;

namespace MultiTenantApp.Web.Models.DTOs
{
    public class VaccineDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Manufacturer { get; set; }
        public int ApplicationAgeMonths { get; set; }
        public int Doses { get; set; }
        public int DoseIntervalDays { get; set; }
        public bool RequiresBooster { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateVaccineDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Manufacturer { get; set; }
        public int ApplicationAgeMonths { get; set; }
        public int Doses { get; set; }
        public int DoseIntervalDays { get; set; }
        public bool RequiresBooster { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateVaccineDto : CreateVaccineDto
    {
    }

    public class VaccineListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Manufacturer { get; set; }
        public int Doses { get; set; }
    }
}
