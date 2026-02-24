using MultiTenantApp.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace MultiTenantApp.Application.Interfaces
{
    public interface IClinicSettingsService
    {
        Task<ClinicSettingsDto> GetSettingsAsync();
        Task UpdateSettingsAsync(ClinicSettingsDto model);
    }
}
