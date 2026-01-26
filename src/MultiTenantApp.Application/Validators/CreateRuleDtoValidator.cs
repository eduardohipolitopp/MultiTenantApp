using FluentValidation;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Resources;

namespace MultiTenantApp.Application.Validators
{
    /// <summary>
    /// Validator for CreateRuleDto using FluentValidation.
    /// </summary>
    public class CreateRuleDtoValidator : AbstractValidator<CreateRuleDto>
    {
        public CreateRuleDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(SharedResource.Required)
                .MinimumLength(2).WithMessage("Name must be at least 2 characters long.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
                .Matches("^[a-zA-Z0-9_ ]+$").WithMessage("Name can only contain letters, numbers, spaces, and underscores.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
        }
    }
}