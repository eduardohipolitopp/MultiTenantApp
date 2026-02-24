using Hangfire;
using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Enums;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Hangfire.Jobs
{
    public class DoseJobs
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMessageService _messageService;
        private readonly ITenantProvider _tenantProvider;

        public DoseJobs(IUnitOfWork unitOfWork, IMessageService messageService, ITenantProvider tenantProvider)
        {
            _unitOfWork = unitOfWork;
            _messageService = messageService;
            _tenantProvider = tenantProvider;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task RunDoseReminders()
        {
            var tenants = await _unitOfWork.Repository<Tenant>().Entities.ToListAsync();
            foreach (var tenant in tenants)
            {
                _tenantProvider.SetTenantId(tenant.Id);
                await SendRemindersForTenant();
            }
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task RunOverdueAlerts()
        {
            var tenants = await _unitOfWork.Repository<Tenant>().Entities.ToListAsync();
            foreach (var tenant in tenants)
            {
                _tenantProvider.SetTenantId(tenant.Id);
                await SendOverdueAlertsForTenant();
            }
        }

        private async Task SendRemindersForTenant()
        {
            var targetDate = DateTime.UtcNow.Date.AddDays(3);
            var appointments = await _unitOfWork.Repository<Appointment>().Entities
                .Include(a => a.Patient)
                .Include(a => a.Vaccine)
                .Where(a => a.ScheduledDateTime.Date == targetDate && a.Status == AppointmentStatus.Scheduled)
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                var template = "Hello {PatientName}, this is a reminder for your {Vaccine} dose scheduled for {ScheduledDate}.";
                var messageDto = await _messageService.CreateAsync(new Application.DTOs.CreateMessageDto
                {
                    PatientId = appointment.PatientId,
                    Channel = MessageChannel.Email,
                    Template = template,
                    AppointmentId = appointment.Id
                });
                await _messageService.SendAsync(messageDto.Id);
            }
        }

        private async Task SendOverdueAlertsForTenant()
        {
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var overdueAppointments = await _unitOfWork.Repository<Appointment>().Entities
                .Include(a => a.Patient)
                .Include(a => a.Vaccine)
                .Where(a => a.ScheduledDateTime.Date <= yesterday && a.Status == AppointmentStatus.Scheduled)
                .ToListAsync();

            foreach (var appointment in overdueAppointments)
            {
                var template = "Hello {PatientName}, you missed your {Vaccine} dose on {ScheduledDate}. Please contact us to reschedule.";
                var messageDto = await _messageService.CreateAsync(new Application.DTOs.CreateMessageDto
                {
                    PatientId = appointment.PatientId,
                    Channel = MessageChannel.Email,
                    Template = template,
                    AppointmentId = appointment.Id
                });
                await _messageService.SendAsync(messageDto.Id);
                
                // Mark as overdue if we had an overdue status, or just keep it scheduled but notify
            }
        }
    }
}
