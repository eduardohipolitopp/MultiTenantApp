using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Enums;
using MultiTenantApp.Domain.Interfaces;
using System.Text.Json;

namespace MultiTenantApp.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;
        private readonly ITenantProvider _tenantProvider;

        public DashboardService(IUnitOfWork unitOfWork, IDistributedCache cache, ITenantProvider tenantProvider)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _tenantProvider = tenantProvider;
        }

        public async Task<DashboardDto> GetDashboardSnapshot()
        {
            var tenantId = _tenantProvider.GetTenantId();
            var cacheKey = $"dashboard_snapshot_{tenantId}";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<DashboardDto>(cachedData) ?? new DashboardDto();
            }

            // Fallback to generating or returning empty
            return await GenerateSnapshotInternal();
        }

        public async Task GenerateDailySnapshot()
        {
            var tenants = await _unitOfWork.Repository<Tenant>().Entities.ToListAsync();
            foreach (var tenant in tenants)
            {
                _tenantProvider.SetTenantId(tenant.Id);
                var snapshot = await GenerateSnapshotInternal();
                
                // Persist to DB
                await _unitOfWork.Repository<DashboardDailySnapshot>().AddAsync(new DashboardDailySnapshot
                {
                    Date = DateTime.UtcNow.Date,
                    ApplicationsToday = snapshot.ApplicationsToday,
                    RevenueToday = snapshot.RevenueToday,
                    OverdueDoses = snapshot.OverdueDoses,
                    ExpiringBatches = snapshot.ExpiringBatches,
                    ScheduledToday = snapshot.ScheduledToday,
                    HomeVisitsMonth = snapshot.HomeVisitsMonth
                });

                // Cache in Redis
                var cacheKey = $"dashboard_snapshot_{tenant.Id}";
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(snapshot), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                });
            }
            
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<DashboardDto> GenerateSnapshotInternal()
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            var applicationsToday = await _unitOfWork.Repository<VaccineApplication>().Entities
                .CountAsync(va => va.ApplicationDate.Date == today);

            var revenueToday = await _unitOfWork.Repository<Finance>().Entities
                .Where(f => f.PaymentDate.Date == today)
                .SumAsync(f => f.Amount);

            var overdueDoses = await _unitOfWork.Repository<Appointment>().Entities
                .CountAsync(a => a.ScheduledDateTime.Date <= yesterday && a.Status == AppointmentStatus.Scheduled);

            var expiringBatches = await _unitOfWork.Repository<VaccineBatch>().Entities
                .CountAsync(b => b.ExpirationDate <= today.AddDays(30) && b.ExpirationDate >= today && b.AvailableQuantity > 0);

            var scheduledToday = await _unitOfWork.Repository<Appointment>().Entities
                .CountAsync(a => a.ScheduledDateTime.Date == today && a.Status == AppointmentStatus.Scheduled);

            var homeVisitsMonth = await _unitOfWork.Repository<VaccineApplication>().Entities
                .CountAsync(va => va.ApplicationDate >= firstDayOfMonth && va.ApplicationType == ApplicationType.HomeVisit);

            return new DashboardDto
            {
                Date = today,
                ApplicationsToday = applicationsToday,
                RevenueToday = revenueToday,
                OverdueDoses = overdueDoses,
                ExpiringBatches = expiringBatches,
                ScheduledToday = scheduledToday,
                HomeVisitsMonth = homeVisitsMonth
            };
        }
    }
}
