using FluentValidation;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Resources;

namespace MultiTenantApp.Application.Validators
{
    /// <summary>
    /// Validator for AssignRuleDto using FluentValidation.
    /// </summary>
    public class AssignRuleDtoValidator : AbstractValidator<AssignRuleDto>
    {
        public AssignRuleDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(SharedResource.Required)
                .Must(BeValidGuid).WithMessage("UserId must be a valid GUID.");

            RuleFor(x => x.RuleId)
                .NotEmpty().WithMessage(SharedResource.Required);

            RuleFor(x => x.PermissionType)
                .InclusiveBetween(1, 2).WithMessage("PermissionType must be 1 (View) or 2 (Edit).");
        }

        private bool BeValidGuid(string userId)
        {
            return Guid.TryParse(userId, out _);
        }
    }
}