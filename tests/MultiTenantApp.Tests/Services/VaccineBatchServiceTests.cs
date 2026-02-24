using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Services;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Persistence;
using MultiTenantApp.Infrastructure.Repositories;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MultiTenantApp.Tests.Services
{
    public class VaccineBatchServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly VaccineBatchService _batchService;
        private readonly IUnitOfWork _unitOfWork;

        public VaccineBatchServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var mockTenantProvider = new Mock<ITenantProvider>();
            mockTenantProvider.Setup(t => t.GetTenantId()).Returns((Guid?)null);

            var mockAuditService = new Mock<IAuditService>();
            var mockCurrentUserService = new Mock<ICurrentUserService>();

            _context = new ApplicationDbContext(options, mockTenantProvider.Object, mockAuditService.Object, mockCurrentUserService.Object);
            
            var services = new ServiceCollection();
            services.AddScoped<ApplicationDbContext>(_ => _context);
            services.AddScoped<IRepository<VaccineBatch>, Repository<VaccineBatch>>();
            var serviceProvider = services.BuildServiceProvider();

            _unitOfWork = new UnitOfWork(_context, serviceProvider);
            _batchService = new VaccineBatchService(_unitOfWork);
        }

        [Fact]
        public async Task GetNextAvailableBatchFIFO_Should_Return_Earliest_Expiring_Batch()
        {
            // Arrange
            var vaccineId = Guid.NewGuid();
            var batches = new List<VaccineBatch>
            {
                new VaccineBatch { Id = Guid.NewGuid(), VaccineId = vaccineId, AvailableQuantity = 10, ExpirationDate = DateTime.UtcNow.AddMonths(6), EntryDate = DateTime.UtcNow.AddDays(-5), BatchNumber = "B1" },
                new VaccineBatch { Id = Guid.NewGuid(), VaccineId = vaccineId, AvailableQuantity = 5, ExpirationDate = DateTime.UtcNow.AddMonths(2), EntryDate = DateTime.UtcNow.AddDays(-2), BatchNumber = "B2" }, // Earliest
                new VaccineBatch { Id = Guid.NewGuid(), VaccineId = vaccineId, AvailableQuantity = 20, ExpirationDate = DateTime.UtcNow.AddMonths(12), EntryDate = DateTime.UtcNow.AddDays(-10), BatchNumber = "B3" }
            };

            await _context.VaccineBatches.AddRangeAsync(batches);
            await _context.SaveChangesAsync();

            // Act
            var result = await _batchService.GetNextAvailableBatchFIFO(vaccineId);

            // Assert
            result.Should().NotBeNull();
            result!.BatchNumber.Should().Be("B2");
        }

        [Fact]
        public async Task GetNextAvailableBatchFIFO_Should_Return_Null_If_No_Stock()
        {
            // Arrange
            var vaccineId = Guid.NewGuid();
            var batches = new List<VaccineBatch>
            {
                new VaccineBatch { Id = Guid.NewGuid(), VaccineId = vaccineId, AvailableQuantity = 0, ExpirationDate = DateTime.UtcNow.AddMonths(6), BatchNumber = "B0" }
            };

            await _context.VaccineBatches.AddRangeAsync(batches);
            await _context.SaveChangesAsync();

            // Act
            var result = await _batchService.GetNextAvailableBatchFIFO(vaccineId);

            // Assert
            result.Should().BeNull();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
