using Hangfire;
using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Enums;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Hangfire.Jobs
{
    public class ClosingJobs
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantProvider _tenantProvider;

        public ClosingJobs(IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
        {
            _unitOfWork = unitOfWork;
            _tenantProvider = tenantProvider;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task RunMonthlyClosing()
        {
            var tenants = await _unitOfWork.Repository<Tenant>().Entities.ToListAsync();
            var lastMonth = DateTime.UtcNow.AddMonths(-1);
            
            foreach (var tenant in tenants)
            {
                _tenantProvider.SetTenantId(tenant.Id);
                await CloseCommissions(lastMonth.Month, lastMonth.Year);
                await CloseHomeVisits(lastMonth.Month, lastMonth.Year);
            }
        }

        private async Task CloseCommissions(int month, int year)
        {
            var alreadyClosed = await _unitOfWork.Repository<CommissionMonthlyClosing>().Entities
                .AnyAsync(c => c.Month == month && c.Year == year);
            
            if (alreadyClosed) return;

            var professionalFinances = await _unitOfWork.Repository<Finance>().Entities
                .Where(f => f.PaymentDate.Month == month && f.PaymentDate.Year == year)
                .GroupBy(f => f.ProfessionalId)
                .Select(g => new
                {
                    ProfessionalId = g.Key,
                    TotalCommission = g.Sum(f => f.CommissionCalculated),
                    Count = g.Count(),
                    HomeVisits = g.Count(f => f.Type == FinanceType.HomeVisit)
                })
                .ToListAsync();

            foreach (var item in professionalFinances)
            {
                await _unitOfWork.Repository<CommissionMonthlyClosing>().AddAsync(new CommissionMonthlyClosing
                {
                    ProfessionalId = item.ProfessionalId,
                    Month = month,
                    Year = year,
                    TotalApplications = item.Count,
                    TotalHomeVisits = item.HomeVisits,
                    CommissionAmount = item.TotalCommission,
                    ClosedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task CloseHomeVisits(int month, int year)
        {
            var alreadyClosed = await _unitOfWork.Repository<HomeVisitMonthlyClosing>().Entities
                .AnyAsync(c => c.Month == month && c.Year == year);
            
            if (alreadyClosed) return;

            var homeVisits = await _unitOfWork.Repository<VaccineApplication>().Entities
                .Where(va => va.ApplicationDate.Month == month && va.ApplicationDate.Year == year && va.ApplicationType == ApplicationType.HomeVisit)
                .GroupBy(va => va.ProfessionalId)
                .Select(g => new
                {
                    ProfessionalId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            foreach (var item in homeVisits)
            {
                await _unitOfWork.Repository<HomeVisitMonthlyClosing>().AddAsync(new HomeVisitMonthlyClosing
                {
                    ProfessionalId = item.ProfessionalId,
                    Month = month,
                    Year = year,
                    TotalVisits = item.Count,
                    BonusAmount = item.Count * 50.00m, // Example bonus: 50 per visit
                    ClosedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
