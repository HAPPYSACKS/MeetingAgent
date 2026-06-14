using FluentAssertions;
using MeetingAgent.Application.Storage;
using MeetingAgent.Domain.Entities;
using MeetingAgent.Domain.Enums;
using MeetingAgent.Domain.ValueObjects;
using MeetingAgent.Infrastructure.Persistence.Repositories;

namespace MeetingAgent.IntegrationTests.Storage;

public sealed class PersistenceRepositoryTests
{
    [LocalDbFact]
    public async Task MeetingSessionRepository_SavesAndLoadsMeetingSession()
    {
        await using var database = new SqlServerTestDatabase();
        await database.MigrateAsync();
        var meeting = CreateMeetingSession();

        await using (var dbContext = database.CreateContext())
        {
            var repository = new MeetingSessionRepository(dbContext);
            await repository.SaveAsync(meeting);
        }

        await using (var dbContext = database.CreateContext())
        {
            var repository = new MeetingSessionRepository(dbContext);
            var loaded = await repository.GetByIdAsync(meeting.Id);

            loaded.Should().NotBeNull();
            loaded!.OrganizerIdentity.Should().Be("host@contoso.com");
            loaded.Title.Should().Be("Weekly sync");
            loaded.TeamsContextIdentifiers.TeamsMeetingId.Should().Be("teams-meeting-123");
        }
    }

    [LocalDbFact]
    public async Task AgendaPlanRepository_SavesVersionHistoryAndLoadsLatestApprovedAgenda()
    {
        await using var database = new SqlServerTestDatabase();
        await database.MigrateAsync();
        var meeting = CreateMeetingSession();
        var draft = CreateAgendaPlan(meeting.Id, 1, AgendaApprovalState.Draft);
        var approved = CreateAgendaPlan(meeting.Id, 2, AgendaApprovalState.Draft);
        approved.Approve(meeting, "host@contoso.com", new DateTimeOffset(2026, 06, 10, 15, 45, 00, TimeSpan.Zero));

        await using (var dbContext = database.CreateContext())
        {
            await new MeetingSessionRepository(dbContext).SaveAsync(meeting);
            var repository = new AgendaPlanRepository(dbContext);
            await repository.SaveAsync(draft);
            await repository.SaveAsync(approved);
        }

        await using (var dbContext = database.CreateContext())
        {
            var repository = new AgendaPlanRepository(dbContext);
            var versions = await repository.ListVersionsAsync(meeting.Id);
            var latestApproved = await repository.GetLatestApprovedAsync(meeting.Id);

            versions.Should().HaveCount(2);
            versions.Select(plan => plan.Version).Should().Equal(1, 2);
            latestApproved.Should().NotBeNull();
            latestApproved!.Version.Should().Be(2);
            latestApproved.ApprovalState.Should().Be(AgendaApprovalState.Approved);
            latestApproved.AgendaSections.Should().HaveCount(2);
        }
    }

    [LocalDbFact]
    public async Task FacilitatorAlertRepository_PreservesDismissedAndResolvedState()
    {
        await using var database = new SqlServerTestDatabase();
        await database.MigrateAsync();
        var meeting = CreateMeetingSession();
        var alert = new FacilitatorAlert(
            Guid.NewGuid(),
            meeting.Id,
            AlertType.PacingRisk,
            AlertSeverity.High,
            new DateTimeOffset(2026, 06, 10, 16, 05, 00, TimeSpan.Zero),
            AlertSource.PacingEngine,
            "The meeting is ten minutes behind the approved agenda.",
            "Move to the decision and park the tangent.",
            false,
            false);
        alert.Dismiss();
        alert.Resolve();

        await using (var dbContext = database.CreateContext())
        {
            await new MeetingSessionRepository(dbContext).SaveAsync(meeting);
            await new FacilitatorAlertRepository(dbContext).SaveAsync(alert);
        }

        await using (var dbContext = database.CreateContext())
        {
            var loaded = await new FacilitatorAlertRepository(dbContext).ListForMeetingAsync(meeting.Id);

            loaded.Should().ContainSingle();
            loaded[0].IsDismissed.Should().BeTrue();
            loaded[0].IsResolved.Should().BeTrue();
        }
    }

    [LocalDbFact]
    public async Task MeetingRecapRepository_SavesDerivedInsightsAndActionItems()
    {
        await using var database = new SqlServerTestDatabase();
        await database.MigrateAsync();
        var meeting = CreateMeetingSession();
        var recap = new MeetingRecap(
            Guid.NewGuid(),
            meeting.Id,
            "The meeting finished on time with one brief topic drift.",
            [new RecapInsight("Budget thread drifted", "Discussion shifted to next quarter staffing.")],
            [new RecapInsight("Clarification loop", "Two attendees repeated the rollout question.")],
            [new RecapActionItem("Send rollout decision summary", "host@contoso.com")],
            new DateTimeOffset(2026, 06, 10, 17, 00, 00, TimeSpan.Zero),
            TranscriptAvailability.Available,
            RecapStatus.Ready);

        await using (var dbContext = database.CreateContext())
        {
            await new MeetingSessionRepository(dbContext).SaveAsync(meeting);
            await new MeetingRecapRepository(dbContext).SaveAsync(recap);
        }

        await using (var dbContext = database.CreateContext())
        {
            var loaded = await new MeetingRecapRepository(dbContext).GetByMeetingIdAsync(meeting.Id);

            loaded.Should().NotBeNull();
            loaded!.TopicDriftMoments.Should().ContainSingle();
            loaded.ConfusionMoments.Should().ContainSingle();
            loaded.ActionItems.Should().ContainSingle();
            loaded.TranscriptAvailability.Should().Be(TranscriptAvailability.Available);
        }
    }

    [LocalDbFact]
    public async Task MeetingRecapRepository_SavesUnavailableTranscriptRecapWithoutTranscriptInsights()
    {
        await using var database = new SqlServerTestDatabase();
        await database.MigrateAsync();
        var meeting = CreateMeetingSession();
        var recap = new MeetingRecap(
            Guid.NewGuid(),
            meeting.Id,
            "Transcript-based insights are unavailable because transcription was disabled.",
            [],
            [],
            [],
            new DateTimeOffset(2026, 06, 10, 17, 00, 00, TimeSpan.Zero),
            TranscriptAvailability.Disabled,
            RecapStatus.Unavailable);

        await using (var dbContext = database.CreateContext())
        {
            await new MeetingSessionRepository(dbContext).SaveAsync(meeting);
            await new MeetingRecapRepository(dbContext).SaveAsync(recap);
        }

        await using (var dbContext = database.CreateContext())
        {
            var loaded = await new MeetingRecapRepository(dbContext).GetByMeetingIdAsync(meeting.Id);

            loaded.Should().NotBeNull();
            loaded!.TranscriptAvailability.Should().Be(TranscriptAvailability.Disabled);
            loaded.Status.Should().Be(RecapStatus.Unavailable);
            loaded.TopicDriftMoments.Should().BeEmpty();
            loaded.ConfusionMoments.Should().BeEmpty();
        }
    }

    [LocalDbFact]
    public async Task TranscriptArtifactRepository_SavesMetadataOnly()
    {
        await using var database = new SqlServerTestDatabase();
        await database.MigrateAsync();
        var meeting = CreateMeetingSession();
        var artifact = new TranscriptArtifactMetadata(
            Guid.NewGuid(),
            meeting.Id,
            "transcript-artifacts",
            "meetings/weekly-sync/transcript.vtt",
            new DateTimeOffset(2026, 06, 10, 17, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2026, 06, 11, 17, 00, 00, TimeSpan.Zero));

        await using (var dbContext = database.CreateContext())
        {
            await new MeetingSessionRepository(dbContext).SaveAsync(meeting);
            await new TranscriptArtifactMetadataRepository(dbContext).SaveAsync(artifact);
        }

        await using (var dbContext = database.CreateContext())
        {
            var loaded = await new TranscriptArtifactMetadataRepository(dbContext).GetByIdAsync(artifact.Id);
            var transcriptProperties = dbContext.Model
                .FindEntityType("MeetingAgent.Infrastructure.Persistence.Records.TranscriptArtifactRecord")!
                .GetProperties()
                .Select(property => property.Name)
                .ToArray();

            loaded.Should().Be(artifact);
            transcriptProperties.Should().NotContain(property =>
                property.Contains("Raw", StringComparison.OrdinalIgnoreCase)
                || property.Contains("Text", StringComparison.OrdinalIgnoreCase)
                || property.Contains("Body", StringComparison.OrdinalIgnoreCase)
                || property.Contains("Content", StringComparison.OrdinalIgnoreCase));
        }
    }

    internal static MeetingSession CreateMeetingSession(DateTimeOffset? scheduledStart = null)
    {
        var start = scheduledStart ?? new DateTimeOffset(2026, 06, 10, 16, 00, 00, TimeSpan.Zero);

        return new MeetingSession(
            Guid.NewGuid(),
            "host@contoso.com",
            "Weekly sync",
            start,
            start.AddMinutes(30),
            MeetingStatus.Scheduled,
            new TeamsMeetingContextIdentifiers("teams-meeting-123", "chat-123", "event-123"));
    }

    internal static AgendaPlan CreateAgendaPlan(Guid meetingId, int version, AgendaApprovalState approvalState)
    {
        return new AgendaPlan(
            Guid.NewGuid(),
            meetingId,
            "Finalize sprint priorities",
            TimeSpan.FromMinutes(30),
            [
                new AgendaSection("Context", "Set up the decision", TimeSpan.FromMinutes(10), 0),
                new AgendaSection("Decision", "Choose the priority list", TimeSpan.FromMinutes(15), 1)
            ],
            version,
            approvalState);
    }
}
