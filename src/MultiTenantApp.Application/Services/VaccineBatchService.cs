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
    public class VaccineBatchService : IVaccineBatchService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VaccineBatchService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<VaccineBatchDto?> GetByIdAsync(Guid id)
        {
            var batch = await _unitOfWork.Repository<VaccineBatch>()
                .Entities
                .Include(b => b.Vaccine)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (batch == null) return null;

            return MapToDto(batch);
        }

        public async Task<VaccineBatchDto> CreateAsync(CreateVaccineBatchDto model)
        {
            var batch = new VaccineBatch
            {
                VaccineId = model.VaccineId,
                BatchNumber = model.BatchNumber,
                TotalQuantity = model.TotalQuantity,
                AvailableQuantity = model.TotalQuantity,
                ExpirationDate = model.ExpirationDate,
                Supplier = model.Supplier,
                Notes = model.Notes
            };

            await _unitOfWork.Repository<VaccineBatch>().AddAsync(batch);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(batch);
        }

        public async Task UpdateAsync(Guid id, UpdateVaccineBatchDto model)
        {
            var batch = await _unitOfWork.Repository<VaccineBatch>().GetByIdAsync(id);
            if (batch == null) throw new KeyNotFoundException("Vaccine batch not found");

            batch.BatchNumber = model.BatchNumber;
            batch.TotalQuantity = model.TotalQuantity;
            batch.AvailableQuantity = model.AvailableQuantity;
            batch.ExpirationDate = model.ExpirationDate;
            batch.Supplier = model.Supplier;
            batch.Notes = model.Notes;
            batch.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<VaccineBatch>().UpdateAsync(batch);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var batch = await _unitOfWork.Repository<VaccineBatch>().GetByIdAsync(id);
            if (batch == null) throw new KeyNotFoundException("Vaccine batch not found");

            await _unitOfWork.Repository<VaccineBatch>().DeleteAsync(batch);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PagedResponse<VaccineBatchListDto>> GetAllAsync(PagedRequest request)
        {
            var query = _unitOfWork.Repository<VaccineBatch>().Entities
                .Include(b => b.Vaccine)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(b => b.BatchNumber.Contains(request.SearchTerm) || 
                                     (b.Vaccine != null && b.Vaccine.Name.Contains(request.SearchTerm)));
            }

            var totalCount = await query.CountAsync();

            var batches = await query
                .OrderBy(b => b.ExpirationDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(b => new VaccineBatchListDto
                {
                    Id = b.Id,
                    VaccineName = b.Vaccine != null ? b.Vaccine.Name : "Unknown",
                    BatchNumber = b.BatchNumber,
                    AvailableQuantity = b.AvailableQuantity,
                    ExpirationDate = b.ExpirationDate
                })
                .ToListAsync();

            return new PagedResponse<VaccineBatchListDto>(batches, request.Page, request.PageSize, totalCount);
        }

        public async Task<VaccineBatchDto?> GetNextAvailableBatchFIFO(Guid vaccineId)
        {
            var batch = await _unitOfWork.Repository<VaccineBatch>()
                .Entities
                .Where(b => b.VaccineId == vaccineId && b.AvailableQuantity > 0 && b.ExpirationDate > DateTime.UtcNow)
                .OrderBy(b => b.ExpirationDate) // FIFO by Expiration Date
                .ThenBy(b => b.EntryDate)
                .FirstOrDefaultAsync();

            return batch != null ? MapToDto(batch) : null;
        }

        private VaccineBatchDto MapToDto(VaccineBatch batch)
        {
            return new VaccineBatchDto
            {
                Id = batch.Id,
                VaccineId = batch.VaccineId,
                VaccineName = batch.Vaccine?.Name,
                BatchNumber = batch.BatchNumber,
                TotalQuantity = batch.TotalQuantity,
                AvailableQuantity = batch.AvailableQuantity,
                ExpirationDate = batch.ExpirationDate,
                EntryDate = batch.EntryDate,
                Supplier = batch.Supplier,
                Notes = batch.Notes
            };
        }
    }
}
