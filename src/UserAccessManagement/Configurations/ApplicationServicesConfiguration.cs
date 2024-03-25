using EntityFrameworkCore.UnitOfWork.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Channels;
using UserAccessManagement.Application.Behaviors;
using UserAccessManagement.Application.Commands;
using UserAccessManagement.BackgroundTasks;
using UserAccessManagement.Infrastructure.Data;
using UserAccessManagement.Infrastructure.External.Services.Employers;
using UserAccessManagement.Infrastructure.External.Services.Users;
using UserAccessManagement.Infrastructure.Helpers;

namespace UserAccessManagement.Configurations
{
    public static class ApplicationServicesConfiguration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddControllers();
            services.AddProblemDetails();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddHostedServices(configuration);
            services.AddMediatorServices();
            services.AddValidationServices();
            services.AddMessagingServices();
            services.AddDataServices();
            services.AddExternalServices(configuration);
            services.AddHelperServices();
            services.AddRoutingServices();
            services.AddVersioningServices();

            return services;
        }

        private static IServiceCollection AddHostedServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedService<BenefitsEnrollmentWorker>();
            services.Configure<BenefitsEnrollmentWorkerOptions>(options =>
            {
                configuration.GetSection("BenefitsEnrollmentWorkerOptions").Bind(options);
            });

            return services;
        }

        private static IServiceCollection AddMediatorServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<Program>();

                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
                cfg.AddOpenBehavior(typeof(ValidatorBehavior<,>));
            });

            return services;
        }

        private static IServiceCollection AddValidationServices(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<Program>();

            return services;
        }

        private static IServiceCollection AddMessagingServices(this IServiceCollection services)
        {
            // System.Threading.Channels has been used to create async communication and background processing.
            // Another technology such as RabbitMQ could have also been used to achieve this purpose.
            services.AddSingleton(Channel.CreateUnbounded<RequestBenefitsEnrollmentCommand>(
                new UnboundedChannelOptions
                {
                    SingleWriter = true,
                    SingleReader = true
                }));

            services.AddSingleton(provider =>
                provider.GetRequiredService<Channel<RequestBenefitsEnrollmentCommand>>().Reader);

            services.AddSingleton(provider =>
                provider.GetRequiredService<Channel<RequestBenefitsEnrollmentCommand>>().Writer);

            return services;
        }

        private static IServiceCollection AddDataServices(this IServiceCollection services)
        {
            // An in-memory database has been used to store the background processing results.
            // Regarding the production environment, a real database MUST be used instead.
            services.AddDbContext<UserAccessManagementContext>(options =>
                options.UseInMemoryDatabase(nameof(UserAccessManagementContext)), ServiceLifetime.Scoped);

            services.AddUnitOfWork<UserAccessManagementContext>(ServiceLifetime.Scoped);

            return services;
        }

        private static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IEmployerService, EmployerService>(client =>
            {
                client.BaseAddress = new Uri(configuration["External:EmployerService:BaseUrl"]);
            });

            services.AddHttpClient<IUserService, UserService>(client =>
            {
                client.BaseAddress = new Uri(configuration["External:UserService:BaseUrl"]);
            });

            return services;
        }

        private static IServiceCollection AddHelperServices(this IServiceCollection services)
        {
            services.AddSingleton<IFileHelper, FileHelper>();

            return services;
        }

        private static IServiceCollection AddRoutingServices(this IServiceCollection services)
        {
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });

            return services;
        }

        private static IServiceCollection AddVersioningServices(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            services.AddVersionedApiExplorer(options =>
            {
                // Format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }

        internal static void CreateDatabase(this IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            using var serviceScope = serviceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();

            var context = serviceScope
                .ServiceProvider
                .GetService<UserAccessManagementContext>();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}
