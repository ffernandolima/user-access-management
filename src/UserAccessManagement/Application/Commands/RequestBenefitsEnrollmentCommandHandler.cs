using MediatR;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace UserAccessManagement.Application.Commands
{
    public class RequestBenefitsEnrollmentCommandHandler : IRequestHandler<RequestBenefitsEnrollmentCommand, bool>
    {
        private readonly ChannelWriter<RequestBenefitsEnrollmentCommand> _producer;

        public RequestBenefitsEnrollmentCommandHandler(ChannelWriter<RequestBenefitsEnrollmentCommand> producer)
        {
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        }

        public async Task<bool> Handle(RequestBenefitsEnrollmentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            await _producer.WriteAsync(request, cancellationToken);

            return true;
        }
    }
}
