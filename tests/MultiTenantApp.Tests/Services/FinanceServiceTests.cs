using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Application.Services;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Enums;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Persistence;
using MultiTenantApp.Infrastructure.Repositories;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MultiTenantApp.Tests.Services
{
    public class FinanceServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly FinanceService _financeService;
        private readonly Mock<IClinicSettingsService> _mockSettings;
        private readonly IUnitOfWork _unitOfWork;

        public FinanceServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var mockTenantProvider = new Mock<ITenantProvider>();
            var mockAuditService = new Mock<IAuditService>();
            var mockCurrentUserService = new Mock<ICurrentUserService>();

            _context = new ApplicationDbContext(options, mockTenantProvider.Object, mockAuditService.Object, mockCurrentUserService.Object);
            _mockSettings = new Mock<IClinicSettingsService>();

            var services = new ServiceCollection();
            services.AddScoped<ApplicationDbContext>(_ => _context);
            services.AddScoped<IRepository<Finance>, Repository<Finance>>();
            services.AddScoped<IRepository<CommissionMonthlyClosing>, Repository<CommissionMonthlyClosing>>();
            var serviceProvider = services.BuildServiceProvider();

            _unitOfWork = new UnitOfWork(_context, serviceProvider);
            _financeService = new FinanceService(_unitOfWork, _mockSettings.Object);
        }

        [Fact]
        public async Task RegisterPayment_Should_Calculate_Commission_Based_On_Settings()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var professionalId = Guid.NewGuid().ToString();

            await _context.Patients.AddAsync(new Patient { Id = patientId, Name = "Test Patient", Phone = "123" });
            await _context.Users.AddAsync(new ApplicationUser { Id = professionalId, UserName = "pro", FullName = "Professional" });
            await _context.SaveChangesAsync();

            var settings = new ClinicSettingsDto { CommissionPercentage = 10, HomeVisitBonus = 50 };
            _mockSettings.Setup(s => s.GetSettingsAsync()).ReturnsAsync(settings);

            var model = new CreateFinanceDto
            {
                PatientId = patientId,
                ProfessionalId = professionalId,
                Amount = 100,
                Type = FinanceType.Clinic,
                PaymentDate = DateTime.UtcNow
            };

            // Act
            var result = await _financeService.RegisterPayment(model);

            // Assert
            result.CommissionCalculated.Should().Be(10); // 10% of 100
        }

        [Fact]
        public async Task RegisterPayment_HomeVisit_Should_Include_Bonus()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var professionalId = Guid.NewGuid().ToString();

            await _context.Patients.AddAsync(new Patient { Id = patientId, Name = "Test Patient", Phone = "123" });
            await _context.Users.AddAsync(new ApplicationUser { Id = professionalId, UserName = "pro", FullName = "Professional" });
            await _context.SaveChangesAsync();

            var settings = new ClinicSettingsDto { CommissionPercentage = 10, HomeVisitBonus = 50 };
            _mockSettings.Setup(s => s.GetSettingsAsync()).ReturnsAsync(settings);

            var model = new CreateFinanceDto
            {
                PatientId = patientId,
                ProfessionalId = professionalId,
                Amount = 100,
                Type = FinanceType.HomeVisit,
                PaymentDate = DateTime.UtcNow
            };

            // Act
            var result = await _financeService.RegisterPayment(model);

            // Assert
            result.CommissionCalculated.Should().Be(60); // (10% of 100) + 50
        }

        [Fact]
        public async Task MonthlyClosing_Should_Throw_If_Already_Closed()
        {
            // Arrange
            var closing = new CommissionMonthlyClosing { Month = 1, Year = 2024, TenantId = Guid.NewGuid() };
            await _context.CommissionMonthlyClosings.AddAsync(closing);
            await _context.SaveChangesAsync();

            // Act & Assert
            Func<Task> act = () => _financeService.MonthlyClosing(1, 2024);
            await act.Should().ThrowAsync<Exception>().WithMessage("*already been processed*");
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
