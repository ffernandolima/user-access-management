using System;

namespace UserAccessManagement.Domain
{
    public class BenefitsEnrollment : Entity
    {
        public override int Id { get; init; }
        public string Email { get; init; }
        public string FullName { get; init; }
        public string Country { get; init; }
        public DateTime? BirthDate { get; init; }
        public decimal? Salary { get; init; }
        public string EmployerName { get; init; }
    }
}
