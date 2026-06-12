using MeetingAgent.Domain.Enums;

namespace MeetingAgent.Infrastructure.Persistence.Records;

public sealed class FacilitatorAlertRecord
{
    public Guid Id { get; set; }

    public Guid MeetingId { get; set; }

    public AlertType AlertType { get; set; }

    public AlertSeverity Severity { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public AlertSource Source { get; set; }

    public string EvidenceSnippet { get; set; } = string.Empty;

    public string Recommendation { get; set; } = string.Empty;

    public bool IsDismissed { get; set; }

    public bool IsResolved { get; set; }

    public MeetingSessionRecord? Meeting { get; set; }
}
