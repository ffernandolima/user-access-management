using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using UserAccessManagement.Application.Commands;

namespace UserAccessManagement.Application.Validators
{
    public class SignUpValidator : AbstractValidator<SignUpCommand>
    {
        public SignUpValidator(ILogger<SignUpValidator> logger)
        {
            ArgumentNullException.ThrowIfNull(logger);

            RuleFor(command => command.Email)
                .NotNull()
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(320);

            RuleFor(command => command.Password)
                .NotNull()
                .NotEmpty()
                .MinimumLength(8).WithMessage("Password length must be at least 8.")
             // .MaximumLength(16).WithMessage("Password length must not exceed 16.") It would be interesting to validate the maximum length as well.
                .Matches(@"[A-Z]+").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]+").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]+").WithMessage("Password must contain at least one number.")
                .Matches(@"[\!\?\*\.]+").WithMessage("Password must contain at least one (!? *.).");

            RuleFor(command => command.Country)
                .NotNull()
                .NotEmpty()
                .MaximumLength(2);

            logger.LogTrace("Instance Created - {ClassName}", nameof(SignUpValidator));
        }
    }
}
