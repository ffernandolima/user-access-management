using MediatR;

namespace UserAccessManagement.Application.Commands
{
    public class SignUpCommand : IRequest<bool>
    {
        public string Email { get; init; }
        public string Password { get; init; }
        public string Country { get; init; }
    }
}
