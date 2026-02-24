using FluentAssertions;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Validators;
using System;
using Xunit;

namespace MultiTenantApp.Tests.Validators
{
    public class VaccineBatchValidatorTests
    {
        private readonly VaccineBatchValidator _validator;

        public VaccineBatchValidatorTests()
        {
            _validator = new VaccineBatchValidator();
        }

        [Fact]
        public void Should_Have_Error_When_BatchNumber_Is_Empty()
        {
            var dto = new CreateVaccineBatchDto { BatchNumber = "", VaccineId = Guid.NewGuid(), TotalQuantity = 10, ExpirationDate = DateTime.Today.AddYears(1) };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "BatchNumber");
        }

        [Fact]
        public void Should_Have_Error_When_ExpirationDate_Is_In_The_Past()
        {
            var dto = new CreateVaccineBatchDto { BatchNumber = "B1", VaccineId = Guid.NewGuid(), TotalQuantity = 10, ExpirationDate = DateTime.Today.AddDays(-1) };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "ExpirationDate");
        }

        [Fact]
        public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
        {
            var dto = new CreateVaccineBatchDto
            {
                VaccineId = Guid.NewGuid(),
                BatchNumber = "B123",
                TotalQuantity = 100,
                ExpirationDate = DateTime.Today.AddMonths(6)
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }
    }
}
