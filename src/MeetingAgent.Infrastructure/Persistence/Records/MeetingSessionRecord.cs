using MeetingAgent.Domain.Enums;

namespace MeetingAgent.Infrastructure.Persistence.Records;

public sealed class MeetingSessionRecord
{
    public Guid Id { get; set; }

    public string OrganizerIdentity { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public DateTimeOffset ScheduledStart { get; set; }

    public DateTimeOffset ScheduledEnd { get; set; }

    public MeetingStatus Status { get; set; }

    public string TeamsMeetingId { get; set; } = string.Empty;

    public string? TeamsChatId { get; set; }

    public string? CalendarEventId { get; set; }

    public List<AgendaPlanRecord> AgendaPlans { get; } = [];

    public List<FacilitatorAlertRecord> FacilitatorAlerts { get; } = [];

    public List<MeetingRecapRecord> MeetingRecaps { get; } = [];

    public List<TranscriptArtifactRecord> TranscriptArtifacts { get; } = [];
}
