using FluentValidation;
using MultiTenantApp.Application.DTOs;
using System;

namespace MultiTenantApp.Application.Validators
{
    public class PatientCreateValidator : AbstractValidator<CreatePatientDto>
    {
        public PatientCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Birth date is required.")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Birth date cannot be in the future.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required.")
                .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters.");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.");

            RuleFor(x => x.GuardianName)
                .MaximumLength(100).WithMessage("Guardian name cannot exceed 100 characters.");

            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.");
        }
    }
}
