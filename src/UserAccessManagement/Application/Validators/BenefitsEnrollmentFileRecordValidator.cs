using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using UserAccessManagement.Application.Models;

namespace UserAccessManagement.Application.Validators
{
    public class BenefitsEnrollmentFileRecordValidator : AbstractValidator<BenefitsEnrollmentFileRecord>
    {
        public BenefitsEnrollmentFileRecordValidator(ILogger<BenefitsEnrollmentFileRecordValidator> logger)
        {
            ArgumentNullException.ThrowIfNull(logger);

            RuleFor(command => command.Email)
                .NotNull()
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(320);

            RuleFor(command => command.Country)
               .NotNull()
               .NotEmpty()
               .MaximumLength(2);

            logger.LogTrace("Instance Created - {ClassName}", nameof(BenefitsEnrollmentFileRecordValidator));
        }
    }
}
