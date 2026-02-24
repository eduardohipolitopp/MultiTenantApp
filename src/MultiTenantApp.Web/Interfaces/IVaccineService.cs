using System;
using System.Threading.Tasks;
using MultiTenantApp.Web.Models.DTOs;

namespace MultiTenantApp.Web.Interfaces
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
