using MediatR;

namespace UserAccessManagement.Application.Commands
{
    public class ProcessBenefitsEnrollmentCommand : IRequest<bool>
    {
        public string File { get; init; }
        public string EmployerName { get; init; }
    }
}
