namespace MeetingAgent.Application.Storage;

public interface IRetentionCleanupService
{
    Task<RetentionCleanupResult> RunCleanupAsync(DateTimeOffset now, CancellationToken cancellationToken = default);
}
