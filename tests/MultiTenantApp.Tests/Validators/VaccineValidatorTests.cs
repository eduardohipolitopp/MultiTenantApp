using FluentAssertions;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Validators;
using Xunit;

namespace MultiTenantApp.Tests.Validators
{
    public class VaccineValidatorTests
    {
        private readonly VaccineValidator _validator;

        public VaccineValidatorTests()
        {
            _validator = new VaccineValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            var dto = new CreateVaccineDto { Name = "" };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Name");
        }

        [Fact]
        public void Should_Have_Error_When_Doses_Is_Zero_Or_Less()
        {
            var dto = new CreateVaccineDto { Name = "BCG", Doses = 0 };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Doses");
        }

        [Fact]
        public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
        {
            var dto = new CreateVaccineDto
            {
                Name = "BCG",
                Doses = 1,
                ApplicationAgeMonths = 0
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }
    }
}
