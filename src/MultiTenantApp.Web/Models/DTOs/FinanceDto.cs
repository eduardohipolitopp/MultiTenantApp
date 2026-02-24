namespace MultiTenantApp.Web.Models.DTOs
{
    public enum FinanceType
    {
        Clinic,
        HomeVisit,
        Sale
    }

    public class FinanceDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string ProfessionalId { get; set; } = string.Empty;
        public string? ProfessionalName { get; set; }
        public Guid? VaccineId { get; set; }
        public string? VaccineName { get; set; }
        public decimal Amount { get; set; }
        public FinanceType Type { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal CommissionCalculated { get; set; }
    }

    public class CreateFinanceDto
    {
        public Guid PatientId { get; set; }
        public string ProfessionalId { get; set; } = string.Empty;
        public Guid? VaccineId { get; set; }
        public decimal Amount { get; set; }
        public FinanceType Type { get; set; }
        public DateTime PaymentDate { get; set; }
    }

    public class FinanceListDto
    {
        public Guid Id { get; set; }
        public string? PatientName { get; set; }
        public string? ProfessionalName { get; set; }
        public string? VaccineName { get; set; }
        public decimal Amount { get; set; }
        public FinanceType Type { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal CommissionCalculated { get; set; }
    }

    public class FinanceSummaryDto
    {
        public decimal TotalAmount { get; set; }
        public decimal TotalCommissions { get; set; }
        public int TransactionCount { get; set; }
    }
}
