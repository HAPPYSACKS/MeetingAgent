using FluentAssertions;
using MeetingAgent.Application.Storage;
using MeetingAgent.Domain.Entities;
using MeetingAgent.Domain.Enums;
using MeetingAgent.Infrastructure.Options;
using MeetingAgent.Infrastructure.Persistence;
using MeetingAgent.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Options;

namespace MeetingAgent.IntegrationTests.Storage;

public sealed class RetentionCleanupServiceTests
{
    [Fact]
    public async Task RunCleanupAsync_RemovesExpiredArtifactsRecapsAndOldMeetingData()
    {
        await using var database = new SqlServerTestDatabase();
        await database.MigrateAsync();
        var now = new DateTimeOffset(2026, 06, 12, 12, 00, 00, TimeSpan.Zero);
        var retainedMeeting = PersistenceRepositoryTests.CreateMeetingSession(now.AddDays(-10));
        var oldMeeting = PersistenceRepositoryTests.CreateMeetingSession(now.AddDays(-200));
        var expiredArtifact = new TranscriptArtifactMetadata(
            Guid.NewGuid(),
            retainedMeeting.Id,
            "transcript-artifacts",
            "expired.vtt",
            now.AddDays(-2),
            now.AddMinutes(-1));
        var recentArtifact = new TranscriptArtifactMetadata(
            Guid.NewGuid(),
            retainedMeeting.Id,
            "transcript-artifacts",
            "recent.vtt",
            now.AddHours(-1),
            now.AddDays(1));
        var expiredRecap = CreateRecap(retainedMeeting.Id, now.AddDays(-31));
        var recentRecap = CreateRecap(retainedMeeting.Id, now.AddDays(-1));

        await using (var dbContext = database.CreateContext())
        {
            var meetingRepository = new MeetingSessionRepository(dbContext);
            await meetingRepository.SaveAsync(retainedMeeting);
            await meetingRepository.SaveAsync(oldMeeting);

            var artifactRepository = new TranscriptArtifactMetadataRepository(dbContext);
            await artifactRepository.SaveAsync(expiredArtifact);
            await artifactRepository.SaveAsync(recentArtifact);

            var recapRepository = new MeetingRecapRepository(dbContext);
            await recapRepository.SaveAsync(expiredRecap);
            await recapRepository.SaveAsync(recentRecap);
        }

        RetentionCleanupResult result;
        await using (var dbContext = database.CreateContext())
        {
            var cleanupService = new RetentionCleanupService(
                dbContext,
                Options.Create(new RetentionOptions
                {
                    TranscriptArtifactDays = 1,
                    RecapArtifactDays = 30,
                    MeetingMetadataDays = 180
                }));

            result = await cleanupService.RunCleanupAsync(now);
        }

        await using (var dbContext = database.CreateContext())
        {
            var meetingRepository = new MeetingSessionRepository(dbContext);
            var artifactRepository = new TranscriptArtifactMetadataRepository(dbContext);
            var recapRepository = new MeetingRecapRepository(dbContext);
            var expiredArtifactAfterCleanup = await artifactRepository.GetByIdAsync(expiredArtifact.Id);
            var recentArtifactAfterCleanup = await artifactRepository.GetByIdAsync(recentArtifact.Id);
            var retainedMeetingAfterCleanup = await meetingRepository.GetByIdAsync(retainedMeeting.Id);
            var oldMeetingAfterCleanup = await meetingRepository.GetByIdAsync(oldMeeting.Id);
            var latestRetainedRecap = await recapRepository.GetByMeetingIdAsync(retainedMeeting.Id);

            result.Should().Be(new RetentionCleanupResult(1, 1, 1));
            expiredArtifactAfterCleanup.Should().BeNull();
            recentArtifactAfterCleanup.Should().NotBeNull();
            latestRetainedRecap.Should().NotBeNull();
            retainedMeetingAfterCleanup.Should().NotBeNull();
            oldMeetingAfterCleanup.Should().BeNull();
        }
    }

    private static MeetingRecap CreateRecap(Guid meetingId, DateTimeOffset generatedAt)
    {
        return new MeetingRecap(
            Guid.NewGuid(),
            meetingId,
            "Pacing summary.",
            [],
            [],
            [],
            generatedAt,
            TranscriptAvailability.Unavailable,
            RecapStatus.Unavailable);
    }
}
