namespace MeetingAgent.Application.Storage;

public interface ITranscriptArtifactMetadataRepository
{
    Task SaveAsync(TranscriptArtifactMetadata artifact, CancellationToken cancellationToken = default);

    Task<TranscriptArtifactMetadata?> GetByIdAsync(Guid artifactId, CancellationToken cancellationToken = default);
}
