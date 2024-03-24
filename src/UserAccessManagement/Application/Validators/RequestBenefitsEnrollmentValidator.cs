using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using UserAccessManagement.Application.Commands;

namespace UserAccessManagement.Application.Validators
{
    public class RequestBenefitsEnrollmentValidator : AbstractValidator<RequestBenefitsEnrollmentCommand>
    {
        public RequestBenefitsEnrollmentValidator(ILogger<RequestBenefitsEnrollmentValidator> logger)
        {
            ArgumentNullException.ThrowIfNull(logger);

            RuleFor(command => command.File)
                .NotNull()
                .NotEmpty();

            RuleFor(command => command.EmployerName)
                .NotNull()
                .NotEmpty();

            logger.LogTrace("Instance Created - {ClassName}", nameof(RequestBenefitsEnrollmentValidator));
        }
    }
}
