using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using UserAccessManagement.Application.Models;
using UserAccessManagement.Application.Validators;
using Xunit;

namespace UserAccessManagement.UnitTests.Application.Validators
{
    public class BenefitsEnrollmentFileRecordValidatorTests
    {
        private readonly Mock<ILogger<BenefitsEnrollmentFileRecordValidator>> _mockLogger;
        private readonly BenefitsEnrollmentFileRecordValidator _validator;

        public BenefitsEnrollmentFileRecordValidatorTests()
        {
            _mockLogger = new Mock<ILogger<BenefitsEnrollmentFileRecordValidator>>();
            _validator = new BenefitsEnrollmentFileRecordValidator(_mockLogger.Object);
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Null()
        {
            var model = new BenefitsEnrollmentFileRecord { Email = null };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Email);
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Empty()
        {
            var model = new BenefitsEnrollmentFileRecord { Email = string.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Email);
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Not_Vdalid()
        {
            var model = new BenefitsEnrollmentFileRecord { Email = "plainaddress" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Email);
        }

        [Fact]
        public void Should_Have_Error_When_Country_Is_Null()
        {
            var model = new BenefitsEnrollmentFileRecord { Country = null };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Country);
        }

        [Fact]
        public void Should_Have_Error_When_Country_Is_Empty()
        {
            var model = new BenefitsEnrollmentFileRecord { Country = string.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Country);
        }

        [Fact]
        public void Should_Have_Error_When_Country_Is_Not_Vdalid()
        {
            var model = new BenefitsEnrollmentFileRecord { Country = "Brazilzilzil" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.Country);
        }
    }
}
