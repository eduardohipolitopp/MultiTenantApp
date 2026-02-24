using System;
using System.Threading.Tasks;
using MultiTenantApp.Web.Models.DTOs;

namespace MultiTenantApp.Web.Interfaces
{
    public interface IPatientService
    {
        Task<PatientDto> GetByIdAsync(Guid id);
        Task<PatientDto> CreateAsync(CreatePatientDto model);
        Task UpdateAsync(Guid id, UpdatePatientDto model);
        Task DeleteAsync(Guid id);
        Task<PagedResponse<PatientListDto>> GetAllAsync(PagedRequest request);
    }
}
