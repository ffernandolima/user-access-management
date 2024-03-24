using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using UserAccessManagement.Application.Commands;

namespace UserAccessManagement.BackgroundTasks
{
    public class BenefitsEnrollmentWorker : BackgroundService
    {
        private readonly ILogger<BenefitsEnrollmentWorker> _logger;
        private readonly IOptions<BenefitsEnrollmentWorkerOptions> _options;
        private readonly ChannelReader<RequestBenefitsEnrollmentCommand> _consumer;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly SemaphoreSlim _semaphore;

        public BenefitsEnrollmentWorker(
            ILogger<BenefitsEnrollmentWorker> logger,
            IOptions<BenefitsEnrollmentWorkerOptions> options,
            ChannelReader<RequestBenefitsEnrollmentCommand> consumer,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _semaphore = new SemaphoreSlim(options.Value.MaxDegreeOfParallelism, options.Value.MaxDegreeOfParallelism);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => _logger.LogDebug($"{nameof(BenefitsEnrollmentWorker)} is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                while (_semaphore.CurrentCount == 0)
                {
                    await Task.Delay(_options.Value.NoAvailableSlotsQueueDelay, stoppingToken);
                }

                var request = await _consumer.ReadAsync(stoppingToken);

                if (request is null)
                {
                    continue;
                }

                await Task.Run(async () =>
                {
                    try
                    {
                        await _semaphore.WaitAsync();

                        using var scope = _serviceScopeFactory.CreateScope();

                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                        await mediator.Send(new ProcessBenefitsEnrollmentCommand
                        {
                            File = request.File,
                            EmployerName = request.EmployerName
                        });
                    }
                    finally
                    {
                        _semaphore.Release();
                    }

                }, stoppingToken);
            }
        }
    }
}
