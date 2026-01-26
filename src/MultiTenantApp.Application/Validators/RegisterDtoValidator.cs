using FluentValidation;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Resources;

namespace MultiTenantApp.Application.Validators
{
    /// <summary>
    /// Validator for RegisterDto using FluentValidation.
    /// </summary>
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(SharedResource.Required)
                .EmailAddress().WithMessage(SharedResource.EmailAddress)
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(SharedResource.Required)
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .MaximumLength(128).WithMessage("Password must not exceed 128 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.TenantId)
                .NotEmpty().WithMessage(SharedResource.Required)
                .Must(BeValidGuid).WithMessage("TenantId must be a valid GUID.");
        }

        private bool BeValidGuid(string tenantId)
        {
            return Guid.TryParse(tenantId, out _);
        }
    }
}