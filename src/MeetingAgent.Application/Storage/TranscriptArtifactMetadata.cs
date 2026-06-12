namespace MeetingAgent.Application.Storage;

public sealed record TranscriptArtifactMetadata(
    Guid Id,
    Guid MeetingId,
    string BlobContainerName,
    string BlobName,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);
