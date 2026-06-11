using MeetingAgent.Domain.Validation;

namespace MeetingAgent.Domain.ValueObjects;

public sealed record TeamsMeetingContextIdentifiers
{
    public TeamsMeetingContextIdentifiers(string teamsMeetingId, string? teamsChatId, string? calendarEventId)
    {
        TeamsMeetingId = DomainRules.Required(teamsMeetingId, nameof(teamsMeetingId));
        TeamsChatId = string.IsNullOrWhiteSpace(teamsChatId) ? null : teamsChatId.Trim();
        CalendarEventId = string.IsNullOrWhiteSpace(calendarEventId) ? null : calendarEventId.Trim();
    }

    public string TeamsMeetingId { get; }

    public string? TeamsChatId { get; }

    public string? CalendarEventId { get; }
}
