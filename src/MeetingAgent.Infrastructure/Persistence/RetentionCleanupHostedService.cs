using MeetingAgent.Application.Storage;
using MeetingAgent.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeetingAgent.Infrastructure.Persistence;

public sealed class RetentionCleanupHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<RetentionCleanupOptions> options,
    ILogger<RetentionCleanupHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cleanupOptions = options.Value;
        if (!cleanupOptions.Enabled)
        {
            logger.LogInformation("Retention cleanup worker is disabled.");
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(cleanupOptions.IntervalMinutes));

        await RunOnceAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunOnceAsync(stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var cleanupService = scope.ServiceProvider.GetRequiredService<IRetentionCleanupService>();
            var result = await cleanupService.RunCleanupAsync(DateTimeOffset.UtcNow, cancellationToken);

            logger.LogInformation(
                "Retention cleanup completed. Transcript artifacts deleted: {TranscriptArtifactsDeleted}; recap artifacts deleted: {RecapArtifactsDeleted}; meeting metadata deleted: {MeetingMetadataDeleted}.",
                result.TranscriptArtifactsDeleted,
                result.RecapArtifactsDeleted,
                result.MeetingMetadataDeleted);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Retention cleanup failed.");
        }
    }
}
