using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantApp.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AppointmentDto?> GetByIdAsync(Guid id)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().Entities
                .Include(a => a.Patient)
                .Include(a => a.Vaccine)
                .Include(a => a.Professional)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return null;

            return MapToDto(appointment);
        }

        public async Task<PagedResponse<AppointmentListDto>> GetAllAsync(PagedRequest request)
        {
            var query = _unitOfWork.Repository<Appointment>().Entities
                .Include(a => a.Patient)
                .Include(a => a.Vaccine)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(a => a.Patient!.Name.Contains(request.SearchTerm) || 
                                     a.Vaccine!.Name.Contains(request.SearchTerm));
            }

            var totalCount = await query.CountAsync();

            var appointments = await query
                .OrderByDescending(a => a.ScheduledDateTime)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new AppointmentListDto
                {
                    Id = a.Id,
                    PatientName = a.Patient!.Name,
                    VaccineName = a.Vaccine!.Name,
                    ScheduledDateTime = a.ScheduledDateTime,
                    Status = a.Status,
                    Type = a.Type
                })
                .ToListAsync();

            return new PagedResponse<AppointmentListDto>(appointments, request.Page, request.PageSize, totalCount);
        }

        public async Task<AppointmentDto> CreateAsync(CreateAppointmentDto model)
        {
            var appointment = new Appointment
            {
                PatientId = model.PatientId,
                VaccineId = model.VaccineId,
                ProfessionalId = model.ProfessionalId,
                ScheduledDateTime = model.ScheduledDateTime,
                Type = model.Type,
                Status = MultiTenantApp.Domain.Enums.AppointmentStatus.Scheduled
            };

            await _unitOfWork.Repository<Appointment>().AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(appointment.Id) ?? throw new Exception("Error creating appointment");
        }

        public async Task UpdateAsync(Guid id, UpdateAppointmentDto model)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(id);
            if (appointment == null) throw new KeyNotFoundException("Appointment not found");

            appointment.ScheduledDateTime = model.ScheduledDateTime;
            appointment.Status = model.Status;
            appointment.Type = model.Type;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Appointment>().UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(id);
            if (appointment == null) throw new KeyNotFoundException("Appointment not found");

            await _unitOfWork.Repository<Appointment>().DeleteAsync(appointment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<AppointmentDto>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            var appointments = await _unitOfWork.Repository<Appointment>().Entities
                .Include(a => a.Patient)
                .Include(a => a.Vaccine)
                .Include(a => a.Professional)
                .Where(a => a.ScheduledDateTime >= start && a.ScheduledDateTime <= end)
                .ToListAsync();

            return appointments.Select(MapToDto);
        }

        private AppointmentDto MapToDto(Appointment appointment)
        {
            return new AppointmentDto
            {
                Id = appointment.Id,
                PatientId = appointment.PatientId,
                PatientName = appointment.Patient?.Name,
                VaccineId = appointment.VaccineId,
                VaccineName = appointment.Vaccine?.Name,
                ProfessionalId = appointment.ProfessionalId,
                ProfessionalName = appointment.Professional?.FullName,
                ScheduledDateTime = appointment.ScheduledDateTime,
                Status = appointment.Status,
                Type = appointment.Type
            };
        }
    }
}
