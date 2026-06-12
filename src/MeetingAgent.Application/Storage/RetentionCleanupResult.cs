namespace MeetingAgent.Application.Storage;

public sealed record RetentionCleanupResult(
    int TranscriptArtifactsDeleted,
    int RecapArtifactsDeleted,
    int MeetingMetadataDeleted);
