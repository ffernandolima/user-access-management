using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using UserAccessManagement.Application.Commands;
using UserAccessManagement.Application.Validators;
using Xunit;

namespace UserAccessManagement.UnitTests.Application.Validators
{
    public class SignUpValidatorTests
    {
        private readonly Mock<ILogger<SignUpValidator>> _mockLogger;
        private readonly SignUpValidator _validator;

        public SignUpValidatorTests()
        {
            _mockLogger = new Mock<ILogger<SignUpValidator>>();
            _validator = new SignUpValidator(_mockLogger.Object);
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Null()
        {
            var model = new SignUpCommand { Email = null };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Email);
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Empty()
        {
            var model = new SignUpCommand { Email = string.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Email);
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Not_Vdalid()
        {
            var model = new SignUpCommand { Email = "plainaddress" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Email);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Null()
        {
            var model = new SignUpCommand { Password = null };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Password);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Empty()
        {
            var model = new SignUpCommand { Password = string.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Password);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Minimum_Length_Is_Not_Valid()
        {
            var model = new SignUpCommand { Password = "pass" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Password);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Does_Not_Have_One_Uppercase_Letter()
        {
            var model = new SignUpCommand { Password = "password" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Password);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Does_Not_Have_One_Lowercase_Letter()
        {
            var model = new SignUpCommand { Password = "PASSWORD" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Password);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Does_Not_Have_One_Number()
        {
            var model = new SignUpCommand { Password = "Password" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Password);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Does_Not_Have_One_Symbol()
        {
            var model = new SignUpCommand { Password = "Password1" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Password);
        }

        [Fact]
        public void Should_Be_A_Valid_Password()
        {
            var model = new SignUpCommand { Password = "x4xxfgKr5T*" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(model => model.Password);
        }

        [Fact]
        public void Should_Have_Error_When_Country_Is_Null()
        {
            var model = new SignUpCommand { Country = null };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Country);
        }

        [Fact]
        public void Should_Have_Error_When_Country_Is_Empty()
        {
            var model = new SignUpCommand { Country = string.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Country);
        }

        [Fact]
        public void Should_Have_Error_When_Country_Is_Not_Vdalid()
        {
            var model = new SignUpCommand { Country = "Brazilzilzil" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Country);
        }
    }
}
