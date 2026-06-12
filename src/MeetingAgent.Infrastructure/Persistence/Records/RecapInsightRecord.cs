namespace MeetingAgent.Infrastructure.Persistence.Records;

public sealed class RecapInsightRecord
{
    public int Id { get; set; }

    public Guid MeetingRecapId { get; set; }

    public RecapInsightKind Kind { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string EvidenceSnippet { get; set; } = string.Empty;

    public MeetingRecapRecord? MeetingRecap { get; set; }
}
