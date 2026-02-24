using FluentAssertions;
using Microsoft.AspNetCore.Identity;
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
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MultiTenantApp.Tests.Services
{
    public class VaccineApplicationServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly VaccineApplicationService _applicationService;
        private readonly Mock<IFinanceService> _mockFinance;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<ITenantProvider> _mockTenantProvider;
        private readonly IUnitOfWork _unitOfWork;

        public VaccineApplicationServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockTenantProvider = new Mock<ITenantProvider>();
            _mockTenantProvider.Setup(t => t.GetTenantId()).Returns((Guid?)null);

            var mockAuditService = new Mock<IAuditService>();
            var mockCurrentUserService = new Mock<ICurrentUserService>();

            _context = new ApplicationDbContext(options, _mockTenantProvider.Object, mockAuditService.Object, mockCurrentUserService.Object);
            _mockFinance = new Mock<IFinanceService>();
            
            // Mock UserManager
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var services = new ServiceCollection();
            services.AddSingleton<ApplicationDbContext>(_ => _context);
            services.AddSingleton<IRepository<VaccineApplication>>(new Repository<VaccineApplication>(_context));
            services.AddSingleton<IRepository<VaccineBatch>>(new Repository<VaccineBatch>(_context));
            services.AddSingleton<IRepository<Vaccine>>(new Repository<Vaccine>(_context));
            services.AddSingleton<IRepository<Appointment>>(new Repository<Appointment>(_context));
            var serviceProvider = services.BuildServiceProvider();

            _unitOfWork = new UnitOfWork(_context, serviceProvider);
            _applicationService = new VaccineApplicationService(_unitOfWork, _mockFinance.Object, _mockUserManager.Object);
        }

        [Fact]
        public async Task ApplyVaccine_Should_Select_FIFO_Batch_And_Decrease_Stock()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            _mockTenantProvider.Setup(t => t.GetTenantId()).Returns(tenantId);
            
            var vaccineId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var professionalId = "nurse-1";

            var batches = new List<VaccineBatch>
            {
                new VaccineBatch { Id = Guid.NewGuid(), VaccineId = vaccineId, AvailableQuantity = 10, ExpirationDate = DateTime.UtcNow.AddMonths(6), BatchNumber = "B1", TenantId = tenantId },
                new VaccineBatch { Id = Guid.NewGuid(), VaccineId = vaccineId, AvailableQuantity = 5, ExpirationDate = DateTime.UtcNow.AddMonths(2), BatchNumber = "B2", TenantId = tenantId } // FIFO
            };
            await _context.VaccineBatches.AddRangeAsync(batches);
            
            var vaccine = new Vaccine { Id = vaccineId, Name = "Vax", Doses = 2, DoseIntervalDays = 30, TenantId = tenantId };
            await _context.Vaccines.AddAsync(vaccine);
            await _context.Patients.AddAsync(new Patient { Id = patientId, Name = "Test Patient", Phone = "123", TenantId = tenantId });
            await _context.SaveChangesAsync();

            // Verify seeding
            var seededBatches = await _context.VaccineBatches.Where(b => b.VaccineId == vaccineId).ToListAsync();
            seededBatches.Should().HaveCount(2);

            var nurse = new ApplicationUser { Id = professionalId, UserName = "nurse", TenantId = tenantId };
            _mockUserManager.Setup(m => m.FindByIdAsync(professionalId)).ReturnsAsync(nurse);
            _mockUserManager.Setup(m => m.GetRolesAsync(nurse)).ReturnsAsync(new List<string> { "Nurse" });

            var model = new CreateVaccineApplicationDto
            {
                PatientId = patientId,
                VaccineId = vaccineId,
                ProfessionalId = professionalId,
                ApplicationDate = DateTime.UtcNow,
                DoseNumber = 1,
                ApplicationType = ApplicationType.Clinic,
                PaidAmount = 100
            };

            // Act
            await _applicationService.ApplyVaccine(model);

            // Assert
            var updatedBatch = await _context.VaccineBatches.FirstAsync(b => b.BatchNumber == "B2");
            updatedBatch.AvailableQuantity.Should().Be(4);

            _mockFinance.Verify(f => f.RegisterPayment(It.IsAny<CreateFinanceDto>()), Times.Once);
            
            var nextAppointment = await _context.Appointments.FirstOrDefaultAsync(a => a.PatientId == patientId);
            _context.Appointments.Count().Should().Be(1);
        }

        [Fact]
        public async Task ApplyVaccine_Should_Throw_If_Dose_Already_Exists()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            _mockTenantProvider.Setup(t => t.GetTenantId()).Returns(tenantId);
            
            var vaccineId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            
            await _context.Vaccines.AddAsync(new Vaccine { Id = vaccineId, Name = "Vax", TenantId = tenantId });
            var batch = new VaccineBatch { Id = Guid.NewGuid(), VaccineId = vaccineId, AvailableQuantity = 10, ExpirationDate = DateTime.UtcNow.AddYears(1), BatchNumber = "B1", TenantId = tenantId };
            await _context.VaccineBatches.AddAsync(batch);
            
            var existingApp = new VaccineApplication { PatientId = patientId, VaccineBatchId = batch.Id, DoseNumber = 1, TenantId = tenantId };
            await _context.VaccineApplications.AddAsync(existingApp);
            await _context.SaveChangesAsync();

            var model = new CreateVaccineApplicationDto
            {
                PatientId = patientId,
                VaccineId = vaccineId,
                DoseNumber = 1
            };

            // Act & Assert
            var act = () => _applicationService.ApplyVaccine(model);
            await act.Should().ThrowAsync<Exception>().Where(e => e.Message.Contains("already received dose 1"));
        }

        [Fact]
        public async Task ApplyVaccine_Should_Throw_If_Role_Unauthorized()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            _mockTenantProvider.Setup(t => t.GetTenantId()).Returns(tenantId);

            var vaccineId = Guid.NewGuid();
            await _context.Vaccines.AddAsync(new Vaccine { Id = vaccineId, Name = "Vax", TenantId = tenantId });
            await _context.VaccineBatches.AddAsync(new VaccineBatch { Id = Guid.NewGuid(), VaccineId = vaccineId, AvailableQuantity = 10, ExpirationDate = DateTime.UtcNow.AddYears(1), BatchNumber = "B1", TenantId = tenantId });
            await _context.SaveChangesAsync();

            var professionalId = "reception-1";
            var professional = new ApplicationUser { Id = professionalId };
            _mockUserManager.Setup(m => m.FindByIdAsync(professionalId)).ReturnsAsync(professional);
            _mockUserManager.Setup(m => m.GetRolesAsync(professional)).ReturnsAsync(new List<string> { "Receptionist" });

            var model = new CreateVaccineApplicationDto
            {
                ProfessionalId = professionalId,
                VaccineId = vaccineId
            };

            // Act & Assert
            var act = () => _applicationService.ApplyVaccine(model);
            await act.Should().ThrowAsync<Exception>().Where(e => e.Message.Contains("Only Nurses or Admins"));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
