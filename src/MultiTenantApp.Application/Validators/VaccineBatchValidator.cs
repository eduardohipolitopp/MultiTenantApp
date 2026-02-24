using FluentValidation;
using MultiTenantApp.Application.DTOs;

namespace MultiTenantApp.Application.Validators
{
    public class VaccineBatchValidator : AbstractValidator<CreateVaccineBatchDto>
    {
        public VaccineBatchValidator()
        {
            RuleFor(x => x.VaccineId)
                .NotEmpty().WithMessage("Vaccine is required");

            RuleFor(x => x.BatchNumber)
                .NotEmpty().WithMessage("Batch Number is required")
                .MaximumLength(50).WithMessage("Batch Number must not exceed 50 characters");

            RuleFor(x => x.TotalQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Total Quantity cannot be negative");

            RuleFor(x => x.ExpirationDate)
                .NotEmpty().WithMessage("Expiration Date is required")
                .Must(date => date > DateTime.Today).WithMessage("Expiration Date must be in the future");
            
            RuleFor(x => x.Supplier)
                .MaximumLength(100).WithMessage("Supplier must not exceed 100 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters");
        }
    }
}
