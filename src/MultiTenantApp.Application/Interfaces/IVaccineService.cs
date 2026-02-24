using MultiTenantApp.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace MultiTenantApp.Application.Interfaces
{
    public interface IVaccineService
    {
        Task<VaccineDto> GetByIdAsync(Guid id);
        Task<VaccineDto> CreateAsync(CreateVaccineDto model);
        Task UpdateAsync(Guid id, UpdateVaccineDto model);
        Task DeleteAsync(Guid id);
        Task<PagedResponse<VaccineListDto>> GetAllAsync(PagedRequest request);
    }
}
