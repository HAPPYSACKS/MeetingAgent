using MeetingAgent.Domain.Enums;

namespace MeetingAgent.Infrastructure.Persistence.Records;

public sealed class MeetingRecapRecord
{
    public Guid Id { get; set; }

    public Guid MeetingId { get; set; }

    public string PacingSummary { get; set; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; set; }

    public TranscriptAvailability TranscriptAvailability { get; set; }

    public RecapStatus Status { get; set; }

    public MeetingSessionRecord? Meeting { get; set; }

    public List<RecapInsightRecord> Insights { get; } = [];

    public List<RecapActionItemRecord> ActionItems { get; } = [];
}
