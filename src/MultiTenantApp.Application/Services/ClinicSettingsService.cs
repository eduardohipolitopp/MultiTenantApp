using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace MultiTenantApp.Application.Services
{
    public class ClinicSettingsService : IClinicSettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;

        public ClinicSettingsService(IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        public async Task<ClinicSettingsDto> GetSettingsAsync()
        {
            var cacheKey = "ClinicSettings";
            var cached = await _cacheService.GetAsync<ClinicSettingsDto>(cacheKey);
            if (cached != null) return cached;

            var settings = await _unitOfWork.Repository<ClinicSettings>().Entities.FirstOrDefaultAsync();
            if (settings == null)
            {
                // Create default if none exists (usually seeded)
                settings = new ClinicSettings
                {
                    ClinicName = "MultiTenant Clinic",
                    CommissionPercentage = 10,
                    HomeVisitBonus = 50,
                    ReminderDaysBefore = 3,
                    ExpirationAlertDays = 30,
                    DefaultCurrency = "BRL"
                };
                await _unitOfWork.Repository<ClinicSettings>().AddAsync(settings);
                await _unitOfWork.SaveChangesAsync();
            }

            var dto = MapToDto(settings);
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromHours(24));
            return dto;
        }

        public async Task UpdateSettingsAsync(ClinicSettingsDto model)
        {
            var settings = await _unitOfWork.Repository<ClinicSettings>().Entities.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new ClinicSettings();
                await _unitOfWork.Repository<ClinicSettings>().AddAsync(settings);
            }

            settings.ClinicName = model.ClinicName;
            settings.CommissionPercentage = model.CommissionPercentage;
            settings.HomeVisitBonus = model.HomeVisitBonus;
            settings.ReminderDaysBefore = model.ReminderDaysBefore;
            settings.ExpirationAlertDays = model.ExpirationAlertDays;
            settings.DefaultCurrency = model.DefaultCurrency;

            await _unitOfWork.Repository<ClinicSettings>().UpdateAsync(settings);
            await _unitOfWork.SaveChangesAsync();

            await _cacheService.RemoveAsync("ClinicSettings");
        }

        private ClinicSettingsDto MapToDto(ClinicSettings s)
        {
            return new ClinicSettingsDto
            {
                ClinicName = s.ClinicName,
                CommissionPercentage = s.CommissionPercentage,
                HomeVisitBonus = s.HomeVisitBonus,
                ReminderDaysBefore = s.ReminderDaysBefore,
                ExpirationAlertDays = s.ExpirationAlertDays,
                DefaultCurrency = s.DefaultCurrency
            };
        }
    }
}
