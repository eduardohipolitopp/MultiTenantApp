using MultiTenantApp.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace MultiTenantApp.Application.Interfaces
{
    public interface IVaccineApplicationService
    {
        Task<VaccineApplicationDto?> GetByIdAsync(Guid id);
        Task<PagedResponse<VaccineApplicationListDto>> GetAllAsync(PagedRequest request);
        Task<VaccineApplicationDto> ApplyVaccine(CreateVaccineApplicationDto model);
    }
}
