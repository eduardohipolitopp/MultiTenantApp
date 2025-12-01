using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Services;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using Xunit;

namespace MultiTenantApp.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _service = new ProductService(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnListOfProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Name = "P1", Price = 10 },
                new Product { Name = "P2", Price = 20 }
            };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("P1");
        }

        [Fact]
        public async Task CreateAsync_ShouldAddProductAndReturnDto()
        {
            // Arrange
            var dto = new CreateProductDto { Name = "New Product", Price = 100 };
            
            _mockRepo.Setup(r => r.AddAsync(It.IsAny<Product>()))
                .Callback<Product>(p => p.Id = Guid.NewGuid()) // Simulate DB generating ID
                .ReturnsAsync((Product p) => p);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(dto.Name);
            result.Id.Should().NotBeEmpty();
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        }
    }
}
