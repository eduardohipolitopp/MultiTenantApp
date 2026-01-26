using FluentValidation;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Resources;

namespace MultiTenantApp.Application.Validators
{
    /// <summary>
    /// Validator for UpdateUserDto using FluentValidation.
    /// </summary>
    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(SharedResource.Required)
                .EmailAddress().WithMessage(SharedResource.EmailAddress)
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");
        }
    }
}