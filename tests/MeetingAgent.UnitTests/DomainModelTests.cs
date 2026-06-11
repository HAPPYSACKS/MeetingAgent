using FluentAssertions;
using MeetingAgent.Domain.Entities;
using MeetingAgent.Domain.Enums;
using MeetingAgent.Domain.ValueObjects;

namespace MeetingAgent.UnitTests;

public class DomainModelTests
{
    [Fact]
    public void MeetingSession_RequiresEndAfterStart()
    {
        var start = new DateTimeOffset(2026, 06, 10, 16, 00, 00, TimeSpan.Zero);

        var act = () => new MeetingSession(
            Guid.NewGuid(),
            "host@contoso.com",
            "Weekly sync",
            start,
            start,
            MeetingStatus.Scheduled,
            new TeamsMeetingContextIdentifiers("meeting-123", "chat-123", "event-123"));

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Scheduled end must be after scheduled start*");
    }

    [Fact]
    public void MeetingSession_RequiresMatchingHostForOwnership()
    {
        var meeting = CreateMeetingSession();

        var act = () => meeting.AssertHostOwnership("other-host@contoso.com");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Only the organizer or authorized host owner may access this meeting*");
    }

    [Fact]
    public void AgendaPlan_RejectsSectionDurationsThatExceedMeetingDuration()
    {
        var sections = new[]
        {
            new AgendaSection("Intro", "Set the stage", TimeSpan.FromMinutes(20), 0),
            new AgendaSection("Deep dive", "Review decisions", TimeSpan.FromMinutes(20), 1)
        };

        var act = () => new AgendaPlan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Finalize next sprint priorities",
            TimeSpan.FromMinutes(30),
            sections,
            1,
            AgendaApprovalState.Draft);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Agenda section durations cannot exceed the total meeting duration*");
    }

    [Fact]
    public void AgendaPlan_Approve_RequiresMeetingOwner()
    {
        var meeting = CreateMeetingSession();
        var plan = CreateAgendaPlan(meeting.Id);

        var act = () => plan.Approve(meeting, "delegate@contoso.com", DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AgendaPlan_Approve_SetsApprovalMetadata()
    {
        var meeting = CreateMeetingSession();
        var plan = CreateAgendaPlan(meeting.Id);
        var approvedAt = new DateTimeOffset(2026, 06, 10, 16, 15, 00, TimeSpan.Zero);

        plan.Approve(meeting, "host@contoso.com", approvedAt);

        plan.ApprovalState.Should().Be(AgendaApprovalState.Approved);
        plan.ApprovedBy.Should().Be("host@contoso.com");
        plan.ApprovedAt.Should().Be(approvedAt);
    }

    [Fact]
    public void AgendaSection_RequiresPositiveDuration()
    {
        var act = () => new AgendaSection("Wrap up", "Confirm next steps", TimeSpan.Zero, 0);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Agenda section duration must be at least*");
    }

    [Fact]
    public void FacilitatorAlert_CanBeDismissedAndResolved()
    {
        var alert = new FacilitatorAlert(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AlertType.PacingRisk,
            AlertSeverity.High,
            DateTimeOffset.UtcNow,
            AlertSource.PacingEngine,
            "The meeting is 10 minutes behind the approved agenda.",
            "Park the current tangent and move to a decision.",
            false,
            false);

        alert.Dismiss();
        alert.Resolve();

        alert.IsDismissed.Should().BeTrue();
        alert.IsResolved.Should().BeTrue();
    }

    [Fact]
    public void MeetingRecap_PreservesTranscriptAvailabilityAndMoments()
    {
        var recap = new MeetingRecap(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "The meeting finished late after one topic drift segment.",
            [new RecapInsight("Budget discussion drifted", "Conversation shifted to next quarter staffing.")],
            [new RecapInsight("Clarification loop", "Two attendees repeated the same question about rollout.")],
            [new RecapActionItem("Send recap to attendees", "host@contoso.com")],
            DateTimeOffset.UtcNow,
            TranscriptAvailability.Available,
            RecapStatus.Ready);

        recap.TranscriptAvailability.Should().Be(TranscriptAvailability.Available);
        recap.TopicDriftMoments.Should().HaveCount(1);
        recap.ConfusionMoments.Should().HaveCount(1);
        recap.ActionItems.Should().HaveCount(1);
    }

    private static MeetingSession CreateMeetingSession()
    {
        return new MeetingSession(
            Guid.NewGuid(),
            "host@contoso.com",
            "Weekly sync",
            new DateTimeOffset(2026, 06, 10, 16, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2026, 06, 10, 16, 30, 00, TimeSpan.Zero),
            MeetingStatus.Scheduled,
            new TeamsMeetingContextIdentifiers("meeting-123", "chat-123", "event-123"));
    }

    private static AgendaPlan CreateAgendaPlan(Guid meetingId)
    {
        return new AgendaPlan(
            Guid.NewGuid(),
            meetingId,
            "Finalize next sprint priorities",
            TimeSpan.FromMinutes(30),
            [
                new AgendaSection("Intro", "Set the stage", TimeSpan.FromMinutes(10), 0),
                new AgendaSection("Decisions", "Make the call", TimeSpan.FromMinutes(15), 1),
                new AgendaSection("Wrap up", "Confirm owners", TimeSpan.FromMinutes(5), 2)
            ],
            1,
            AgendaApprovalState.Draft);
    }
}
