using FluentValidation;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Resources;

namespace MultiTenantApp.Application.Validators
{
    /// <summary>
    /// Validator for CreateProductDto using FluentValidation.
    /// </summary>
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(SharedResource.Required)
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0.");
        }
    }
}
