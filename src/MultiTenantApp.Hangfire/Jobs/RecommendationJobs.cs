using Hangfire;
using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Enums;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Hangfire.Jobs
{
    public class RecommendationJobs
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMessageService _messageService;
        private readonly ITenantProvider _tenantProvider;

        public RecommendationJobs(IUnitOfWork unitOfWork, IMessageService messageService, ITenantProvider tenantProvider)
        {
            _unitOfWork = unitOfWork;
            _messageService = messageService;
            _tenantProvider = tenantProvider;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task RunVaccineByAgeRecommendations()
        {
            var tenants = await _unitOfWork.Repository<Tenant>().Entities.ToListAsync();
            foreach (var tenant in tenants)
            {
                _tenantProvider.SetTenantId(tenant.Id);
                await ProcessRecommendations();
            }
        }

        private async Task ProcessRecommendations()
        {
            var patients = await _unitOfWork.Repository<Patient>().Entities.ToListAsync();
            var vaccines = await _unitOfWork.Repository<Vaccine>().Entities.ToListAsync();

            foreach (var patient in patients)
            {
                // Simple age calculation in months
                var birthDate = patient.BirthDate;
                var ageInMonths = ((DateTime.UtcNow.Year - birthDate.Year) * 12) + DateTime.UtcNow.Month - birthDate.Month;

                foreach (var vaccine in vaccines)
                {
                    if (vaccine.ApplicationAgeMonths <= ageInMonths)
                    {
                        // Check if patient already has this vaccine
                        var alreadyApplied = await _unitOfWork.Repository<VaccineApplication>().Entities
                            .AnyAsync(va => va.PatientId == patient.Id && va.VaccineBatch!.VaccineId == vaccine.Id);

                        if (!alreadyApplied)
                        {
                            var messageDto = await _messageService.CreateAsync(new Application.DTOs.CreateMessageDto
                            {
                                PatientId = patient.Id,
                                Channel = MessageChannel.Email,
                                Template = $"Hello {{PatientName}}, based on your age, we recommend the {vaccine.Name} vaccine. Contact us to schedule."
                            });
                            await _messageService.SendAsync(messageDto.Id);
                        }
                    }
                }
            }
        }
    }
}
