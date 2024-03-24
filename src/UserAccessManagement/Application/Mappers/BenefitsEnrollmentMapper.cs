using System;
using UserAccessManagement.Application.Models;
using UserAccessManagement.Domain;

namespace UserAccessManagement.Application.Mappers
{
    public class BenefitsEnrollmentMapper
    {
        public static BenefitsEnrollment Map(BenefitsEnrollmentFileRecord record, string employerName)
        {
            ArgumentNullException.ThrowIfNull(record);

            var entity = new BenefitsEnrollment
            {
                Email = record.Email,
                FullName = record.FullName,
                Country = record.Country,
                BirthDate = record.BirthDate,
                Salary = record.Salary,
                EmployerName = employerName
            };

            return entity;
        }
    }
}
