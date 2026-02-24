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
    public class FinanceService : IFinanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClinicSettingsService _clinicSettingsService;

        public FinanceService(IUnitOfWork unitOfWork, IClinicSettingsService clinicSettingsService)
        {
            _unitOfWork = unitOfWork;
            _clinicSettingsService = clinicSettingsService;
        }

        public async Task<FinanceDto?> GetByIdAsync(Guid id)
        {
            var finance = await _unitOfWork.Repository<Finance>().Entities
                .Include(f => f.Patient)
                .Include(f => f.Professional)
                .Include(f => f.Vaccine)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (finance == null) return null;

            return MapToDto(finance);
        }

        public async Task<PagedResponse<FinanceListDto>> GetAllAsync(PagedRequest request)
        {
            var query = _unitOfWork.Repository<Finance>().Entities
                .Include(f => f.Patient)
                .Include(f => f.Professional)
                .Include(f => f.Vaccine)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(f => f.Patient!.Name.Contains(request.SearchTerm) || 
                                     f.Professional!.FullName!.Contains(request.SearchTerm));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(f => f.PaymentDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(f => new FinanceListDto
                {
                    Id = f.Id,
                    PatientName = f.Patient!.Name,
                    ProfessionalName = f.Professional!.FullName ?? f.Professional.UserName,
                    VaccineName = f.Vaccine != null ? f.Vaccine.Name : "N/A",
                    Amount = f.Amount,
                    Type = f.Type,
                    PaymentDate = f.PaymentDate,
                    CommissionCalculated = f.CommissionCalculated
                })
                .ToListAsync();

            return new PagedResponse<FinanceListDto>(items, request.Page, request.PageSize, totalCount);
        }

        public async Task<FinanceDto> RegisterPayment(CreateFinanceDto model)
        {
            var finance = new Finance
            {
                PatientId = model.PatientId,
                ProfessionalId = model.ProfessionalId,
                VaccineId = model.VaccineId,
                Amount = model.Amount,
                Type = model.Type,
                PaymentDate = model.PaymentDate
            };

            // Calculate commission
            finance.CommissionCalculated = await CalculateCommissionInternal(finance.Amount, finance.Type);

            await _unitOfWork.Repository<Finance>().AddAsync(finance);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(finance.Id) ?? throw new Exception("Error registering payment");
        }

        public async Task<decimal> CalculateCommission(Guid financeId)
        {
            var finance = await _unitOfWork.Repository<Finance>().GetByIdAsync(financeId);
            if (finance == null) return 0;

            var commission = await CalculateCommissionInternal(finance.Amount, finance.Type);
            finance.CommissionCalculated = commission;
            
            await _unitOfWork.Repository<Finance>().UpdateAsync(finance);
            await _unitOfWork.SaveChangesAsync();
            
            return commission;
        }

        private async Task<decimal> CalculateCommissionInternal(decimal amount, Domain.Enums.FinanceType type)
        {
            var settings = await _clinicSettingsService.GetSettingsAsync();
            var percentage = settings.CommissionPercentage;

            return type switch
            {
                Domain.Enums.FinanceType.Sale => amount * (percentage / 100),
                Domain.Enums.FinanceType.Clinic => amount * (percentage / 100),
                Domain.Enums.FinanceType.HomeVisit => (amount * (percentage / 100)) + settings.HomeVisitBonus,
                _ => 0
            };
        }

        public async Task<FinanceSummaryDto> GetSummary(DateTime? start = null, DateTime? end = null)
        {
            var query = _unitOfWork.Repository<Finance>().Entities.AsQueryable();

            if (start.HasValue) query = query.Where(f => f.PaymentDate >= start.Value);
            if (end.HasValue) query = query.Where(f => f.PaymentDate <= end.Value);

            return new FinanceSummaryDto
            {
                TotalAmount = await query.SumAsync(f => f.Amount),
                TotalCommissions = await query.SumAsync(f => f.CommissionCalculated),
                TransactionCount = await query.CountAsync()
            };
        }

        public async Task<bool> MonthlyClosing(int month, int year)
        {
            // Validation: Prevent double closing (Checking if a closing record already exists)
            var closingExists = await _unitOfWork.Repository<CommissionMonthlyClosing>().Entities
                .AnyAsync(c => c.Month == month && c.Year == year);

            if (closingExists)
                throw new Exception($"Monthly closing for {month}/{year} has already been processed.");

            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            var query = _unitOfWork.Repository<Finance>().Entities
                .Where(f => f.PaymentDate >= start && f.PaymentDate <= end);

            return await query.AnyAsync();
        }

        private FinanceDto MapToDto(Finance finance)
        {
            return new FinanceDto
            {
                Id = finance.Id,
                PatientId = finance.PatientId,
                PatientName = finance.Patient?.Name,
                ProfessionalId = finance.ProfessionalId,
                ProfessionalName = finance.Professional?.FullName ?? finance.Professional?.UserName,
                VaccineId = finance.VaccineId,
                VaccineName = finance.Vaccine?.Name,
                Amount = finance.Amount,
                Type = finance.Type,
                PaymentDate = finance.PaymentDate,
                CommissionCalculated = finance.CommissionCalculated
            };
        }
    }
}
