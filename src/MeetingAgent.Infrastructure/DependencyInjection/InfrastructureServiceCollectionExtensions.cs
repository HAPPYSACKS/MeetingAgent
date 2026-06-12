using MeetingAgent.Application.Storage;
using MeetingAgent.Infrastructure.Options;
using MeetingAgent.Infrastructure.Persistence;
using MeetingAgent.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace MeetingAgent.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<RetentionOptions>()
            .Bind(configuration.GetSection(RetentionOptions.SectionName))
            .ValidateDataAnnotations();

        services
            .AddOptions<RetentionCleanupOptions>()
            .Bind(configuration.GetSection(RetentionCleanupOptions.SectionName))
            .ValidateDataAnnotations();

        services
            .AddOptions<SqlOptions>()
            .Bind(configuration.GetSection(SqlOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddDbContext<MeetingAgentDbContext>((serviceProvider, options) =>
        {
            var sqlOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SqlOptions>>().Value;
            options.UseSqlServer(
                MeetingAgentConnectionString.Resolve(configuration),
                sqlServerOptions => sqlServerOptions.CommandTimeout(sqlOptions.CommandTimeoutSeconds));
        });

        services.AddScoped<IMeetingSessionRepository, MeetingSessionRepository>();
        services.AddScoped<IAgendaPlanRepository, AgendaPlanRepository>();
        services.AddScoped<IFacilitatorAlertRepository, FacilitatorAlertRepository>();
        services.AddScoped<IMeetingRecapRepository, MeetingRecapRepository>();
        services.AddScoped<ITranscriptArtifactMetadataRepository, TranscriptArtifactMetadataRepository>();
        services.AddScoped<IRetentionCleanupService, RetentionCleanupService>();

        return services;
    }

    public static IServiceCollection AddRetentionCleanupWorker(this IServiceCollection services)
    {
        services.AddHostedService<RetentionCleanupHostedService>();
        return services;
    }
}
