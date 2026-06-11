using MeetingAgent.Domain.Enums;
using MeetingAgent.Domain.Validation;
using MeetingAgent.Domain.ValueObjects;

namespace MeetingAgent.Domain.Entities;

public sealed class MeetingSession
{
    public MeetingSession(
        Guid id,
        string organizerIdentity,
        string title,
        DateTimeOffset scheduledStart,
        DateTimeOffset scheduledEnd,
        MeetingStatus status,
        TeamsMeetingContextIdentifiers teamsContextIdentifiers)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Meeting id is required.", nameof(id));
        }

        DomainRules.EnsureEnumValue(status, nameof(status));

        if (scheduledEnd <= scheduledStart)
        {
            throw new ArgumentException("Scheduled end must be after scheduled start.", nameof(scheduledEnd));
        }

        var duration = scheduledEnd - scheduledStart;
        DomainRules.EnsureMeetingDuration(duration, "scheduledDuration");

        Id = id;
        OrganizerIdentity = DomainRules.Required(organizerIdentity, nameof(organizerIdentity));
        Title = DomainRules.Required(title, nameof(title));
        ScheduledStart = scheduledStart;
        ScheduledEnd = scheduledEnd;
        Status = status;
        TeamsContextIdentifiers = teamsContextIdentifiers ?? throw new ArgumentNullException(nameof(teamsContextIdentifiers));
    }

    public Guid Id { get; }

    public string OrganizerIdentity { get; }

    public string Title { get; }

    public DateTimeOffset ScheduledStart { get; }

    public DateTimeOffset ScheduledEnd { get; }

    public MeetingStatus Status { get; private set; }

    public TeamsMeetingContextIdentifiers TeamsContextIdentifiers { get; }

    public TimeSpan ScheduledDuration => ScheduledEnd - ScheduledStart;

    public void AssertHostOwnership(string hostIdentity)
    {
        var normalizedHostIdentity = DomainRules.Required(hostIdentity, nameof(hostIdentity));

        if (!string.Equals(OrganizerIdentity, normalizedHostIdentity, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only the organizer or authorized host owner may access this meeting.");
        }
    }

    public void MarkStatus(MeetingStatus status)
    {
        DomainRules.EnsureEnumValue(status, nameof(status));
        Status = status;
    }
}
