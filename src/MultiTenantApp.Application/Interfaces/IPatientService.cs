using System;
using System.Threading.Tasks;
using MultiTenantApp.Application.DTOs;

namespace MultiTenantApp.Application.Interfaces
{
    public interface IPatientService
    {
        Task<PagedResponse<PatientListDto>> GetAllAsync(PagedRequest request);
        Task<PatientDto> GetByIdAsync(Guid id);
        Task<PatientDto> CreateAsync(CreatePatientDto model);
        Task UpdateAsync(Guid id, UpdatePatientDto model);
        Task DeleteAsync(Guid id);
    }
}
