using FluentValidation;
using MultiTenantApp.Application.DTOs;

namespace MultiTenantApp.Application.Validators
{
    public class AppointmentValidator : AbstractValidator<CreateAppointmentDto>
    {
        public AppointmentValidator()
        {
            RuleFor(x => x.PatientId).NotEmpty();
            RuleFor(x => x.VaccineId).NotEmpty();
            RuleFor(x => x.ProfessionalId).NotEmpty();
            RuleFor(x => x.ScheduledDateTime)
                .NotEmpty()
                .Must(x => x > DateTime.UtcNow).WithMessage("Scheduled date must be in the future.");
            RuleFor(x => x.Type).IsInEnum();
        }
    }

    public class UpdateAppointmentValidator : AbstractValidator<UpdateAppointmentDto>
    {
        public UpdateAppointmentValidator()
        {
            RuleFor(x => x.ScheduledDateTime)
                .NotEmpty()
                .Must(x => x > DateTime.UtcNow).WithMessage("Scheduled date must be in the future.");
            RuleFor(x => x.Status).IsInEnum();
            RuleFor(x => x.Type).IsInEnum();
        }
    }
}
