using FluentAssertions;
using MeetingAgent.Application.Pacing;
using MeetingAgent.Domain.Entities;
using MeetingAgent.Domain.Enums;
using MeetingAgent.Domain.ValueObjects;

namespace MeetingAgent.UnitTests;

public sealed class PacingEngineTests
{
    private readonly PacingEngine _engine = new();

    [Fact]
    public void Calculate_ReturnsLowRiskWhenMeetingIsOnTrack()
    {
        var now = DateTimeOffset.UtcNow;
        var meeting = CreateMeeting(now.AddMinutes(-10), now.AddMinutes(20));
        var agenda = CreateAgenda(meeting.Id, 30);

        var snapshot = _engine.Calculate(meeting, agenda, now);

        snapshot.RiskLevel.Should().Be(PacingRiskLevel.Low);
        snapshot.ElapsedMinutes.Should().Be(10);
        snapshot.RemainingMinutes.Should().Be(20);
    }

    [Fact]
    public void Calculate_ReturnsHighRiskNearEndgame()
    {
        var now = DateTimeOffset.UtcNow;
        var meeting = CreateMeeting(now.AddMinutes(-26), now.AddMinutes(4));
        var agenda = CreateAgenda(meeting.Id, 30);

        var snapshot = _engine.Calculate(meeting, agenda, now);

        snapshot.RiskLevel.Should().Be(PacingRiskLevel.High);
        snapshot.SuggestedIntervention.Should().Contain("Protect the close");
    }

    [Fact]
    public void Calculate_ReturnsPastEndAfterScheduledEnd()
    {
        var now = DateTimeOffset.UtcNow;
        var meeting = CreateMeeting(now.AddMinutes(-40), now.AddMinutes(-10));
        var agenda = CreateAgenda(meeting.Id, 30);

        var snapshot = _engine.Calculate(meeting, agenda, now);

        snapshot.HostStatus.Should().Be(MeetingStatus.Completed);
        snapshot.RiskLevel.Should().Be(PacingRiskLevel.PastEnd);
        snapshot.RemainingMinutes.Should().Be(0);
    }

    [Fact]
    public void Calculate_HighlightsCurrentAgendaSection()
    {
        var now = DateTimeOffset.UtcNow;
        var meeting = CreateMeeting(now.AddMinutes(-17), now.AddMinutes(13));
        var agenda = CreateAgenda(meeting.Id, 30);

        var snapshot = _engine.Calculate(meeting, agenda, now);

        snapshot.Sections.Single(section => section.IsCurrent).Title.Should().Be("Decide");
    }

    private static MeetingSession CreateMeeting(DateTimeOffset scheduledStart, DateTimeOffset scheduledEnd)
    {
        return new MeetingSession(
            Guid.NewGuid(),
            "host@example.com",
            "Pilot planning",
            scheduledStart,
            scheduledEnd,
            MeetingStatus.Scheduled,
            new TeamsMeetingContextIdentifiers("meeting-id", "chat-id", "event-id"));
    }

    private static AgendaPlan CreateAgenda(Guid meetingId, int durationMinutes)
    {
        return new AgendaPlan(
            Guid.NewGuid(),
            meetingId,
            "Decide pilot scope",
            TimeSpan.FromMinutes(durationMinutes),
            [
                new AgendaSection("Frame", "Set context", TimeSpan.FromMinutes(10), 0),
                new AgendaSection("Decide", "Make the call", TimeSpan.FromMinutes(15), 1),
                new AgendaSection("Close", "Confirm owners", TimeSpan.FromMinutes(5), 2)
            ],
            1,
            AgendaApprovalState.Draft);
    }
}
