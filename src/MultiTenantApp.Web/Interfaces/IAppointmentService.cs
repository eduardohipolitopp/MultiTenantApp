using MultiTenantApp.Web.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantApp.Web.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentDto?> GetByIdAsync(Guid id);
        Task<PagedResponse<AppointmentListDto>> GetAllAsync(PagedRequest request);
        Task<IEnumerable<AppointmentDto>> GetByDateRangeAsync(DateTime start, DateTime end);
        Task<AppointmentDto> CreateAsync(CreateAppointmentDto model);
        Task UpdateAsync(Guid id, UpdateAppointmentDto model);
        Task DeleteAsync(Guid id);
    }
}
