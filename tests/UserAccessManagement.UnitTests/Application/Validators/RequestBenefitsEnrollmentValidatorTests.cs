using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using UserAccessManagement.Application.Commands;
using UserAccessManagement.Application.Validators;
using Xunit;

namespace UserAccessManagement.UnitTests.Application.Validators
{
    public class RequestBenefitsEnrollmentValidatorTests
    {
        private readonly Mock<ILogger<RequestBenefitsEnrollmentValidator>> _mockLogger;
        private readonly RequestBenefitsEnrollmentValidator _validator;

        public RequestBenefitsEnrollmentValidatorTests()
        {
            _mockLogger = new Mock<ILogger<RequestBenefitsEnrollmentValidator>>();
            _validator = new RequestBenefitsEnrollmentValidator(_mockLogger.Object);
        }

        [Fact]
        public void Should_Have_Error_When_File_Is_Null()
        {
            var model = new RequestBenefitsEnrollmentCommand { File = null };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.File);
        }

        [Fact]
        public void Should_Have_Error_When_File_Is_Empty()
        {
            var model = new RequestBenefitsEnrollmentCommand { File = string.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.File);
        }

        [Fact]
        public void Should_Have_Error_When_Employer_Name_Is_Null()
        {
            var model = new RequestBenefitsEnrollmentCommand { EmployerName = null };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.EmployerName);
        }

        [Fact]
        public void Should_Have_Error_When_Employer_Name_Is_Empty()
        {
            var model = new RequestBenefitsEnrollmentCommand { EmployerName = string.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(model => model.EmployerName);
        }
    }
}
