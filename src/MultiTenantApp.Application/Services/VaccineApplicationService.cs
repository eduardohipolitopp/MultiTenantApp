using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Enums;
using MultiTenantApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantApp.Application.Services
{
    public class VaccineApplicationService : IVaccineApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFinanceService _financeService;
        private readonly UserManager<ApplicationUser> _userManager;

        public VaccineApplicationService(
            IUnitOfWork unitOfWork, 
            IFinanceService financeService,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _financeService = financeService;
            _userManager = userManager;
        }

        public async Task<VaccineApplicationDto?> GetByIdAsync(Guid id)
        {
            var application = await _unitOfWork.Repository<VaccineApplication>().Entities
                .Include(va => va.Patient)
                .Include(va => va.VaccineBatch)
                    .ThenInclude(vb => vb!.Vaccine)
                .Include(va => va.Professional)
                .FirstOrDefaultAsync(va => va.Id == id);

            if (application == null) return null;

            return MapToDto(application);
        }

        public async Task<PagedResponse<VaccineApplicationListDto>> GetAllAsync(PagedRequest request)
        {
            var query = _unitOfWork.Repository<VaccineApplication>().Entities
                .Include(va => va.Patient)
                .Include(va => va.VaccineBatch)
                    .ThenInclude(vb => vb!.Vaccine)
                .Include(va => va.Professional)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(va => va.Patient!.Name.Contains(request.SearchTerm) || 
                                          va.VaccineBatch!.Vaccine!.Name.Contains(request.SearchTerm));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(va => va.ApplicationDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(va => new VaccineApplicationListDto
                {
                    Id = va.Id,
                    PatientName = va.Patient!.Name,
                    VaccineName = va.VaccineBatch!.Vaccine!.Name,
                    BatchNumber = va.VaccineBatch.BatchNumber,
                    ApplicationDate = va.ApplicationDate,
                    DoseNumber = va.DoseNumber,
                    ProfessionalName = va.Professional!.FullName ?? va.Professional.UserName
                })
                .ToListAsync();

            return new PagedResponse<VaccineApplicationListDto>(items, request.Page, request.PageSize, totalCount);
        }

        public async Task<VaccineApplicationDto> ApplyVaccine(CreateVaccineApplicationDto model)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Find FIFO Batch
                var batch = await _unitOfWork.Repository<VaccineBatch>().Entities
                    .Where(vb => vb.VaccineId == model.VaccineId && vb.AvailableQuantity > 0)
                    .OrderBy(vb => vb.ExpirationDate)
                    .ThenBy(vb => vb.EntryDate)
                    .FirstOrDefaultAsync();

                if (batch == null) throw new Exception("No available batches for this vaccine.");

                // Professional Validation: Only Nurse or Admin can apply
                var professional = await _userManager.FindByIdAsync(model.ProfessionalId);
                if (professional == null) throw new Exception("Professional not found.");
                
                var roles = await _userManager.GetRolesAsync(professional);
                if (!roles.Any(r => r == "Nurse" || r == "Admin" || r == "Manager"))
                    throw new Exception("Only Nurses or Admins can apply vaccines.");

                // Validation: Expired Batch
                if (batch.ExpirationDate < DateTime.UtcNow.Date)
                    throw new Exception($"Cannot apply expired vaccine. Batch {batch.BatchNumber} expired on {batch.ExpirationDate:d}");

                // Validation: Duplicate Dose
                var doseExists = await _unitOfWork.Repository<VaccineApplication>().Entities
                    .Include(va => va.VaccineBatch)
                    .AnyAsync(va => va.PatientId == model.PatientId && 
                                   va.VaccineBatch != null &&
                                   va.VaccineBatch.VaccineId == model.VaccineId && 
                                   va.DoseNumber == model.DoseNumber);
                
                if (doseExists)
                    throw new Exception($"Patient already received dose {model.DoseNumber} of this vaccine.");

                // 2. Decrease Stock
                batch.AvailableQuantity -= 1;
                await _unitOfWork.Repository<VaccineBatch>().UpdateAsync(batch);

                // 3. Create Application Record
                var application = new VaccineApplication
                {
                    PatientId = model.PatientId,
                    VaccineBatchId = batch.Id,
                    ApplicationDate = model.ApplicationDate,
                    DoseNumber = model.DoseNumber,
                    ProfessionalId = model.ProfessionalId,
                    ApplicationType = model.ApplicationType,
                    PaidAmount = model.PaidAmount
                };
                await _unitOfWork.Repository<VaccineApplication>().AddAsync(application);

                // 4. Create Finance Entry
                await _financeService.RegisterPayment(new CreateFinanceDto
                {
                    PatientId = model.PatientId,
                    ProfessionalId = model.ProfessionalId,
                    VaccineId = model.VaccineId,
                    Amount = model.PaidAmount,
                    Type = model.ApplicationType == ApplicationType.Clinic ? FinanceType.Clinic : FinanceType.HomeVisit,
                    PaymentDate = model.ApplicationDate
                });

                // 5. Create Next Appointment (if applicable)
                var vaccine = await _unitOfWork.Repository<Vaccine>().GetByIdAsync(model.VaccineId);
                if (vaccine != null && vaccine.Doses > model.DoseNumber)
                {
                    var nextAppointment = new Appointment
                    {
                        PatientId = model.PatientId,
                        VaccineId = model.VaccineId,
                        ProfessionalId = model.ProfessionalId,
                        ScheduledDateTime = model.ApplicationDate.AddDays(vaccine.DoseIntervalDays),
                        Status = AppointmentStatus.Scheduled,
                        Type = model.ApplicationType == ApplicationType.Clinic ? AppointmentType.Clinic : AppointmentType.HomeVisit
                    };
                    await _unitOfWork.Repository<Appointment>().AddAsync(nextAppointment);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return await GetByIdAsync(application.Id) ?? throw new Exception("Error retrieving applied vaccine record.");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private VaccineApplicationDto MapToDto(VaccineApplication va)
        {
            return new VaccineApplicationDto
            {
                Id = va.Id,
                PatientId = va.PatientId,
                PatientName = va.Patient?.Name,
                VaccineBatchId = va.VaccineBatchId,
                BatchNumber = va.VaccineBatch?.BatchNumber,
                VaccineName = va.VaccineBatch?.Vaccine?.Name,
                ApplicationDate = va.ApplicationDate,
                DoseNumber = va.DoseNumber,
                ProfessionalId = va.ProfessionalId,
                ProfessionalName = va.Professional?.FullName ?? va.Professional?.UserName,
                ApplicationType = va.ApplicationType,
                PaidAmount = va.PaidAmount
            };
        }
    }
}
