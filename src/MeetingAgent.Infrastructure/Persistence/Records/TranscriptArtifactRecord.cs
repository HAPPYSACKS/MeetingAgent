namespace MeetingAgent.Infrastructure.Persistence.Records;

public sealed class TranscriptArtifactRecord
{
    public Guid Id { get; set; }

    public Guid MeetingId { get; set; }

    public string BlobContainerName { get; set; } = string.Empty;

    public string BlobName { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public MeetingSessionRecord? Meeting { get; set; }
}
