namespace MeetingAgent.Infrastructure.Persistence.Records;

public sealed class RecapActionItemRecord
{
    public int Id { get; set; }

    public Guid MeetingRecapId { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? Owner { get; set; }

    public MeetingRecapRecord? MeetingRecap { get; set; }
}
