using MultiTenantApp.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace MultiTenantApp.Application.Interfaces
{
    public interface IVaccineBatchService
    {
        Task<VaccineBatchDto?> GetByIdAsync(Guid id);
        Task<VaccineBatchDto> CreateAsync(CreateVaccineBatchDto model);
        Task UpdateAsync(Guid id, UpdateVaccineBatchDto model);
        Task DeleteAsync(Guid id);
        Task<PagedResponse<VaccineBatchListDto>> GetAllAsync(PagedRequest request);
        Task<VaccineBatchDto?> GetNextAvailableBatchFIFO(Guid vaccineId);
    }
}
