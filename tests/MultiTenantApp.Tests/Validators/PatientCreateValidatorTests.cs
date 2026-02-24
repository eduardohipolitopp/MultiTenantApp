using FluentAssertions;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Validators;
using System;
using Xunit;

namespace MultiTenantApp.Tests.Validators
{
    public class PatientCreateValidatorTests
    {
        private readonly PatientCreateValidator _validator;

        public PatientCreateValidatorTests()
        {
            _validator = new PatientCreateValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            var dto = new CreatePatientDto { Name = "" };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Name");
        }

        [Fact]
        public void Should_Have_Error_When_BirthDate_Is_In_Future()
        {
            var dto = new CreatePatientDto { BirthDate = DateTime.UtcNow.AddDays(1) };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "BirthDate");
        }

        [Fact]
        public void Should_Have_Error_When_Phone_Is_Empty()
        {
            var dto = new CreatePatientDto { Phone = "" };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Phone");
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Invalid()
        {
            var dto = new CreatePatientDto { Email = "invalid-email" };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        [Fact]
        public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
        {
            var dto = new CreatePatientDto
            {
                Name = "John Doe",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                Phone = "123456789",
                Gender = 1
            };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }
    }
}
