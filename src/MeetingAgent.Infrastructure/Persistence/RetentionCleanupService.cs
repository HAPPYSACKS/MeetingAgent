using MeetingAgent.Application.Storage;
using MeetingAgent.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MeetingAgent.Infrastructure.Persistence;

public sealed class RetentionCleanupService(
    MeetingAgentDbContext dbContext,
    IOptions<RetentionOptions> retentionOptions) : IRetentionCleanupService
{
    public async Task<RetentionCleanupResult> RunCleanupAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var retention = retentionOptions.Value;
        var transcriptCutoff = now;
        var recapCutoff = now.AddDays(-retention.RecapArtifactDays);
        var meetingMetadataCutoff = now.AddDays(-retention.MeetingMetadataDays);

        var expiredTranscriptArtifacts = await dbContext.TranscriptArtifacts
            .Where(artifact => artifact.ExpiresAt <= transcriptCutoff)
            .ToListAsync(cancellationToken);
        var expiredRecaps = await dbContext.MeetingRecaps
            .Where(recap => recap.GeneratedAt <= recapCutoff)
            .ToListAsync(cancellationToken);
        var expiredMeetings = await dbContext.MeetingSessions
            .Where(meeting => meeting.ScheduledEnd <= meetingMetadataCutoff)
            .ToListAsync(cancellationToken);

        dbContext.TranscriptArtifacts.RemoveRange(expiredTranscriptArtifacts);
        dbContext.MeetingRecaps.RemoveRange(expiredRecaps);
        dbContext.MeetingSessions.RemoveRange(expiredMeetings);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RetentionCleanupResult(
            expiredTranscriptArtifacts.Count,
            expiredRecaps.Count,
            expiredMeetings.Count);
    }
}
