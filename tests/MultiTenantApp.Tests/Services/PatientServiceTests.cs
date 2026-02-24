using FluentAssertions;
using Moq;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Services;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace MultiTenantApp.Tests.Services
{
    public class PatientServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly PatientService _patientService;

        public PatientServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _patientService = new PatientService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task CreateAsync_Should_Add_Patient_And_Save_Changes()
        {
            // Arrange
            var dto = new CreatePatientDto
            {
                Name = "John Doe",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                Gender = 1,
                Phone = "123456789"
            };

            var mockRepo = new Mock<IRepository<Patient>>();
            _mockUnitOfWork.Setup(u => u.Repository<Patient>()).Returns(mockRepo.Object);

            // Act
            var result = await _patientService.CreateAsync(dto);

            // Assert
            mockRepo.Verify(r => r.AddAsync(It.IsAny<Patient>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            result.Name.Should().Be(dto.Name);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Dto_When_Patient_Exists()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var patient = new Patient { Id = patientId, Name = "John Doe" };

            var mockRepo = new Mock<IRepository<Patient>>();
            mockRepo.Setup(r => r.GetByIdAsync(patientId)).ReturnsAsync(patient);
            _mockUnitOfWork.Setup(u => u.Repository<Patient>()).Returns(mockRepo.Object);

            // Act
            var result = await _patientService.GetByIdAsync(patientId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(patientId);
            result.Name.Should().Be(patient.Name);
        }
    }
}
