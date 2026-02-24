using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Enums;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Application.Services
{
    public class PatientService : IPatientService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PatientService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResponse<PatientListDto>> GetAllAsync(PagedRequest request)
        {
            System.Linq.Expressions.Expression<Func<Patient, bool>> filter = null;
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                filter = p => p.Name.Contains(request.SearchTerm) || p.Phone.Contains(request.SearchTerm);
            }

            var (patients, totalCount) = await _unitOfWork.Repository<Patient>().GetPagedAsync(request.Page, request.PageSize, filter);

            var patientDtos = patients.Select(p => new PatientListDto
            {
                Id = p.Id,
                Name = p.Name,
                BirthDate = p.BirthDate,
                Phone = p.Phone
            }).ToList();

            return new PagedResponse<PatientListDto>(patientDtos, request.Page, request.PageSize, totalCount);
        }

        public async Task<PatientDto> GetByIdAsync(Guid id)
        {
            var p = await _unitOfWork.Repository<Patient>().GetByIdAsync(id);
            if (p == null) return null;

            return MapToDto(p);
        }

        public async Task<PatientDto> CreateAsync(CreatePatientDto model)
        {
            var patient = new Patient
            {
                Name = model.Name,
                BirthDate = model.BirthDate,
                Gender = (Gender)model.Gender,
                GuardianName = model.GuardianName,
                Phone = model.Phone,
                Email = model.Email,
                Address = model.Address,
                Notes = model.Notes
            };

            await _unitOfWork.Repository<Patient>().AddAsync(patient);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(patient);
        }

        public async Task UpdateAsync(Guid id, UpdatePatientDto model)
        {
            var repository = _unitOfWork.Repository<Patient>();
            var patient = await repository.GetByIdAsync(id);
            if (patient == null)
            {
                throw new KeyNotFoundException($"Patient with ID {id} not found.");
            }

            patient.Name = model.Name;
            patient.BirthDate = model.BirthDate;
            patient.Gender = (Gender)model.Gender;
            patient.GuardianName = model.GuardianName;
            patient.Phone = model.Phone;
            patient.Email = model.Email;
            patient.Address = model.Address;
            patient.Notes = model.Notes;

            await repository.UpdateAsync(patient);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var repository = _unitOfWork.Repository<Patient>();
            var patient = await repository.GetByIdAsync(id);
            if (patient != null)
            {
                var hasApplications = await _unitOfWork.Repository<VaccineApplication>().Entities
                    .AnyAsync(va => va.PatientId == id);
                
                if (hasApplications)
                    throw new Exception("Cannot delete patient with existing vaccine applications.");

                await repository.DeleteAsync(patient);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        private PatientDto MapToDto(Patient p)
        {
            return new PatientDto
            {
                Id = p.Id,
                Name = p.Name,
                BirthDate = p.BirthDate,
                Gender = p.Gender.ToString(),
                GuardianName = p.GuardianName,
                Phone = p.Phone,
                Email = p.Email,
                Address = p.Address,
                Notes = p.Notes
            };
        }
    }
}
