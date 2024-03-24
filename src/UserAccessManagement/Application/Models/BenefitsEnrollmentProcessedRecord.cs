using FileHelpers;
using FluentValidation.Results;
using System;
using System.Linq;

namespace UserAccessManagement.Application.Models
{
    [IgnoreEmptyLines]
    [DelimitedRecord("|")]
    public class BenefitsEnrollmentProcessedRecord
    {
        public int LineNumber { get; init; }
        public string Description { get; init; }

        private BenefitsEnrollmentProcessedRecord()
        { }

        public BenefitsEnrollmentProcessedRecord(int lineNumber, string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException($"'{nameof(description)}' cannot be null or whitespace.", nameof(description));
            }

            LineNumber = lineNumber;
            Description = description;
        }

        public BenefitsEnrollmentProcessedRecord(int lineNumber, Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            LineNumber = lineNumber;
            Description = exception.Message;
        }

        public BenefitsEnrollmentProcessedRecord(int lineNumber, ValidationResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            LineNumber = lineNumber;
            Description = $"Validation failed: {string.Join(";",
                result.Errors!.Select(x => $"{x.PropertyName}: {x.ErrorMessage} Severity: {x.Severity}"))}";
        }

        public BenefitsEnrollmentProcessedRecord(ErrorInfo error)
        {
            ArgumentNullException.ThrowIfNull(error);

            LineNumber = error.LineNumber;
            Description = error.ExceptionInfo!.Message;
        }
    }
}
