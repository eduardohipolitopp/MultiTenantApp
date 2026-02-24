using FluentAssertions;
using Moq;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Services;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MultiTenantApp.Tests.Services
{
    public class VaccineServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly VaccineService _vaccineService;

        public VaccineServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _vaccineService = new VaccineService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task CreateAsync_Should_Add_Vaccine_And_Save_Changes()
        {
            // Arrange
            var dto = new CreateVaccineDto
            {
                Name = "BCG",
                Manufacturer = "Manufacturer X",
                Doses = 1
            };

            var mockRepo = new Mock<IRepository<Vaccine>>();
            _mockUnitOfWork.Setup(u => u.Repository<Vaccine>()).Returns(mockRepo.Object);

            // Act
            var result = await _vaccineService.CreateAsync(dto);

            // Assert
            mockRepo.Verify(r => r.AddAsync(It.IsAny<Vaccine>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            result.Name.Should().Be(dto.Name);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Dto_When_Vaccine_Exists()
        {
            // Arrange
            var vaccineId = Guid.NewGuid();
            var vaccine = new Vaccine { Id = vaccineId, Name = "BCG" };

            var mockRepo = new Mock<IRepository<Vaccine>>();
            mockRepo.Setup(r => r.GetByIdAsync(vaccineId)).ReturnsAsync(vaccine);
            _mockUnitOfWork.Setup(u => u.Repository<Vaccine>()).Returns(mockRepo.Object);

            // Act
            var result = await _vaccineService.GetByIdAsync(vaccineId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(vaccineId);
            result.Name.Should().Be(vaccine.Name);
        }
    }
}
