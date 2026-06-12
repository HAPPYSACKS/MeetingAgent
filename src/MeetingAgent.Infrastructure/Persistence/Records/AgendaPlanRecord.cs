using MeetingAgent.Domain.Enums;

namespace MeetingAgent.Infrastructure.Persistence.Records;

public sealed class AgendaPlanRecord
{
    public Guid Id { get; set; }

    public Guid MeetingId { get; set; }

    public string Objective { get; set; } = string.Empty;

    public TimeSpan TotalDuration { get; set; }

    public int Version { get; set; }

    public AgendaApprovalState ApprovalState { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public MeetingSessionRecord? Meeting { get; set; }

    public List<AgendaSectionRecord> AgendaSections { get; } = [];
}
