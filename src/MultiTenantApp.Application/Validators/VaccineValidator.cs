using FluentValidation;
using MultiTenantApp.Application.DTOs;

namespace MultiTenantApp.Application.Validators
{
    public class VaccineValidator : AbstractValidator<CreateVaccineDto>
    {
        public VaccineValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

            RuleFor(x => x.Doses)
                .GreaterThan(0).WithMessage("Doses must be greater than 0");

            RuleFor(x => x.Manufacturer)
                .MaximumLength(100).WithMessage("Manufacturer must not exceed 100 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters");
        }
    }
}
