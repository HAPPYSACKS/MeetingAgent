namespace MeetingAgent.Infrastructure.Persistence.Records;

public sealed class AgendaSectionRecord
{
    public int Id { get; set; }

    public Guid AgendaPlanId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;

    public TimeSpan SuggestedDuration { get; set; }

    public int Order { get; set; }

    public string? FacilitationNotes { get; set; }

    public AgendaPlanRecord? AgendaPlan { get; set; }
}
