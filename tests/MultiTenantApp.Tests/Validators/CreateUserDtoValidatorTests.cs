using FluentAssertions;
using FluentValidation;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Validators;
using Xunit;

namespace MultiTenantApp.Tests.Validators
{
    public class CreateUserDtoValidatorTests
    {
        private readonly CreateUserDtoValidator _validator;

        public CreateUserDtoValidatorTests()
        {
            _validator = new CreateUserDtoValidator();
        }

        [Fact]
        public void Should_Have_Error_When_UserName_Is_Empty()
        {
            // Arrange
            var dto = new CreateUserDto { UserName = "" };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "UserName");
        }

        [Fact]
        public void Should_Have_Error_When_UserName_Is_Too_Short()
        {
            // Arrange
            var dto = new CreateUserDto { UserName = "ab" };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "UserName");
        }

        [Fact]
        public void Should_Have_Error_When_UserName_Has_Invalid_Characters()
        {
            // Arrange
            var dto = new CreateUserDto { UserName = "user@name" };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "UserName");
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Invalid()
        {
            // Arrange
            var dto = new CreateUserDto { Email = "invalid-email" };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Weak()
        {
            // Arrange
            var dto = new CreateUserDto { Password = "weak" };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Password");
        }

        [Fact]
        public void Should_Have_Error_When_TenantId_Is_Invalid()
        {
            // Arrange
            var dto = new CreateUserDto { TenantId = "not-a-guid" };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "TenantId");
        }

        [Fact]
        public void Should_Have_Error_When_Role_Is_Invalid()
        {
            // Arrange
            var dto = new CreateUserDto { Role = "InvalidRole" };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Role");
        }

        [Fact]
        public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
        {
            // Arrange
            var dto = new CreateUserDto
            {
                UserName = "validuser",
                Email = "user@example.com",
                Password = "StrongPass123!",
                TenantId = Guid.NewGuid().ToString(),
                Role = "User"
            };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }
}