﻿using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using UserAccessManagement.Infrastructure.Extensions;

namespace UserAccessManagement.Application.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestType = request.GetType();
            var requestTypeName = requestType.ExtractTypeName();

            _logger.LogInformation("Handling command {CommandName} ({@Command})", requestTypeName, request);

            var response = await next();

            _logger.LogInformation("Command {CommandName} handled - response: {@Response}", requestTypeName, response);

            return response;
        }
    }
}
