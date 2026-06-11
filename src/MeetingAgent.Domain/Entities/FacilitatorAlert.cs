using MeetingAgent.Domain.Enums;
using MeetingAgent.Domain.Validation;

namespace MeetingAgent.Domain.Entities;

public sealed class FacilitatorAlert
{
    public FacilitatorAlert(
        Guid id,
        Guid meetingId,
        AlertType alertType,
        AlertSeverity severity,
        DateTimeOffset timestamp,
        AlertSource source,
        string evidenceSnippet,
        string recommendation,
        bool isDismissed,
        bool isResolved)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Alert id is required.", nameof(id));
        }

        if (meetingId == Guid.Empty)
        {
            throw new ArgumentException("Meeting id is required.", nameof(meetingId));
        }

        DomainRules.EnsureEnumValue(alertType, nameof(alertType));
        DomainRules.EnsureEnumValue(severity, nameof(severity));
        DomainRules.EnsureEnumValue(source, nameof(source));

        Id = id;
        MeetingId = meetingId;
        AlertType = alertType;
        Severity = severity;
        Timestamp = timestamp;
        Source = source;
        EvidenceSnippet = DomainRules.Required(evidenceSnippet, nameof(evidenceSnippet));
        Recommendation = DomainRules.Required(recommendation, nameof(recommendation));
        IsDismissed = isDismissed;
        IsResolved = isResolved;
    }

    public Guid Id { get; }

    public Guid MeetingId { get; }

    public AlertType AlertType { get; }

    public AlertSeverity Severity { get; }

    public DateTimeOffset Timestamp { get; }

    public AlertSource Source { get; }

    public string EvidenceSnippet { get; }

    public string Recommendation { get; }

    public bool IsDismissed { get; private set; }

    public bool IsResolved { get; private set; }

    public void Dismiss()
    {
        IsDismissed = true;
    }

    public void Resolve()
    {
        IsResolved = true;
    }
}
