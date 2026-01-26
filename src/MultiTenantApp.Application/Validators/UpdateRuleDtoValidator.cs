using FluentValidation;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Resources;

namespace MultiTenantApp.Application.Validators
{
    /// <summary>
    /// Validator for UpdateRuleDto using FluentValidation.
    /// </summary>
    public class UpdateRuleDtoValidator : AbstractValidator<UpdateRuleDto>
    {
        public UpdateRuleDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(SharedResource.Required)
                .MinimumLength(2).WithMessage("Name must be at least 2 characters long.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
                .Matches("^[a-zA-Z0-9_ ]+$").WithMessage("Name can only contain letters, numbers, spaces, and underscores.");
        }
    }
}