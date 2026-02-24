using MultiTenantApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantApp.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentDto?> GetByIdAsync(Guid id);
        Task<PagedResponse<AppointmentListDto>> GetAllAsync(PagedRequest request);
        Task<AppointmentDto> CreateAsync(CreateAppointmentDto model);
        Task UpdateAsync(Guid id, UpdateAppointmentDto model);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<AppointmentDto>> GetByDateRangeAsync(DateTime start, DateTime end);
    }
}
