using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Application.Services;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Persistence;
using MultiTenantApp.Infrastructure.Repositories;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MultiTenantApp.Tests.Services
{
    public class ClinicSettingsServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ClinicSettingsService _settingsService;
        private readonly Mock<ICacheService> _mockCache;
        private readonly IUnitOfWork _unitOfWork;

        public ClinicSettingsServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var mockTenantProvider = new Mock<ITenantProvider>();
            var mockAuditService = new Mock<IAuditService>();
            var mockCurrentUserService = new Mock<ICurrentUserService>();

            _context = new ApplicationDbContext(options, mockTenantProvider.Object, mockAuditService.Object, mockCurrentUserService.Object);
            _mockCache = new Mock<ICacheService>();

            var services = new ServiceCollection();
            services.AddScoped<ApplicationDbContext>(_ => _context);
            services.AddScoped<IRepository<ClinicSettings>, Repository<ClinicSettings>>();
            var serviceProvider = services.BuildServiceProvider();

            _unitOfWork = new UnitOfWork(_context, serviceProvider);
            _settingsService = new ClinicSettingsService(_unitOfWork, _mockCache.Object);
        }

        [Fact]
        public async Task GetSettingsAsync_Should_Return_From_Cache_If_Available()
        {
            // Arrange
            var cachedSettings = new ClinicSettingsDto { ClinicName = "Cached Clinic" };
            _mockCache.Setup(c => c.GetAsync<ClinicSettingsDto>("ClinicSettings"))
                .ReturnsAsync(cachedSettings);

            // Act
            var result = await _settingsService.GetSettingsAsync();

            // Assert
            result.ClinicName.Should().Be("Cached Clinic");
            _mockCache.Verify(c => c.GetAsync<ClinicSettingsDto>("ClinicSettings"), Times.Once);
        }

        [Fact]
        public async Task GetSettingsAsync_Should_Create_Default_If_None_Exists()
        {
            // Arrange
            _mockCache.Setup(c => c.GetAsync<ClinicSettingsDto>(It.IsAny<string>()))
                .ReturnsAsync((ClinicSettingsDto?)null);

            // Act
            var result = await _settingsService.GetSettingsAsync();

            // Assert
            result.Should().NotBeNull();
            result.ClinicName.Should().Be("MultiTenant Clinic");
            
            var dbSettings = await _context.ClinicSettings.FirstOrDefaultAsync();
            dbSettings.Should().NotBeNull();
            dbSettings!.ClinicName.Should().Be("MultiTenant Clinic");
        }

        [Fact]
        public async Task UpdateSettingsAsync_Should_Update_Db_And_Clear_Cache()
        {
            // Arrange
            var initialSettings = new ClinicSettings { ClinicName = "Old Name", TenantId = Guid.NewGuid() };
            await _context.ClinicSettings.AddAsync(initialSettings);
            await _context.SaveChangesAsync();

            var updateModel = new ClinicSettingsDto 
            { 
                ClinicName = "New Name",
                CommissionPercentage = 15,
                DefaultCurrency = "USD"
            };

            // Act
            await _settingsService.UpdateSettingsAsync(updateModel);

            // Assert
            var dbSettings = await _context.ClinicSettings.FirstOrDefaultAsync();
            dbSettings!.ClinicName.Should().Be("New Name");
            dbSettings.CommissionPercentage.Should().Be(15);
            
            _mockCache.Verify(c => c.RemoveAsync("ClinicSettings"), Times.Once);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
