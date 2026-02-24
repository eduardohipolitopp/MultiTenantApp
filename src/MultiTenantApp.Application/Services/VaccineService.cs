using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantApp.Application.Services
{
    public class VaccineService : IVaccineService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VaccineService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<VaccineDto> GetByIdAsync(Guid id)
        {
            var vaccine = await _unitOfWork.Repository<Vaccine>().GetByIdAsync(id);
            if (vaccine == null) return null;

            return MapToDto(vaccine);
        }

        public async Task<VaccineDto> CreateAsync(CreateVaccineDto model)
        {
            var vaccine = new Vaccine
            {
                Name = model.Name,
                Manufacturer = model.Manufacturer,
                ApplicationAgeMonths = model.ApplicationAgeMonths,
                Doses = model.Doses,
                DoseIntervalDays = model.DoseIntervalDays,
                RequiresBooster = model.RequiresBooster,
                Notes = model.Notes
            };

            await _unitOfWork.Repository<Vaccine>().AddAsync(vaccine);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(vaccine);
        }

        public async Task UpdateAsync(Guid id, UpdateVaccineDto model)
        {
            var vaccine = await _unitOfWork.Repository<Vaccine>().GetByIdAsync(id);
            if (vaccine == null) throw new Exception("Vaccine not found");

            vaccine.Name = model.Name;
            vaccine.Manufacturer = model.Manufacturer;
            vaccine.ApplicationAgeMonths = model.ApplicationAgeMonths;
            vaccine.Doses = model.Doses;
            vaccine.DoseIntervalDays = model.DoseIntervalDays;
            vaccine.RequiresBooster = model.RequiresBooster;
            vaccine.Notes = model.Notes;
            vaccine.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Vaccine>().UpdateAsync(vaccine);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var vaccine = await _unitOfWork.Repository<Vaccine>().GetByIdAsync(id);
            if (vaccine == null) throw new Exception("Vaccine not found");

            await _unitOfWork.Repository<Vaccine>().DeleteAsync(vaccine);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PagedResponse<VaccineListDto>> GetAllAsync(PagedRequest request)
        {
            System.Linq.Expressions.Expression<Func<Vaccine, bool>> filter = null;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                filter = v => v.Name.Contains(request.SearchTerm) || 
                             (v.Manufacturer != null && v.Manufacturer.Contains(request.SearchTerm));
            }

            var (vaccines, totalCount) = await _unitOfWork.Repository<Vaccine>().GetPagedAsync(request.Page, request.PageSize, filter);

            var items = vaccines.Select(v => new VaccineListDto
            {
                Id = v.Id,
                Name = v.Name,
                Manufacturer = v.Manufacturer,
                Doses = v.Doses
            }).ToList();

            return new PagedResponse<VaccineListDto>(items, request.Page, request.PageSize, totalCount);
        }

        private VaccineDto MapToDto(Vaccine vaccine)
        {
            return new VaccineDto
            {
                Id = vaccine.Id,
                Name = vaccine.Name,
                Manufacturer = vaccine.Manufacturer,
                ApplicationAgeMonths = vaccine.ApplicationAgeMonths,
                Doses = vaccine.Doses,
                DoseIntervalDays = vaccine.DoseIntervalDays,
                RequiresBooster = vaccine.RequiresBooster,
                Notes = vaccine.Notes
            };
        }
    }
}
