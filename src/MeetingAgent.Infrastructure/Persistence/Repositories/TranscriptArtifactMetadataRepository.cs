using MeetingAgent.Application.Storage;
using Microsoft.EntityFrameworkCore;

namespace MeetingAgent.Infrastructure.Persistence.Repositories;

public sealed class TranscriptArtifactMetadataRepository(MeetingAgentDbContext dbContext) : ITranscriptArtifactMetadataRepository
{
    public async Task SaveAsync(TranscriptArtifactMetadata artifact, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(artifact);

        var existing = await dbContext.TranscriptArtifacts
            .SingleOrDefaultAsync(record => record.Id == artifact.Id, cancellationToken);

        if (existing is null)
        {
            dbContext.TranscriptArtifacts.Add(MeetingAgentPersistenceMapper.ToRecord(artifact));
        }
        else
        {
            existing.MeetingId = artifact.MeetingId;
            existing.BlobContainerName = artifact.BlobContainerName;
            existing.BlobName = artifact.BlobName;
            existing.CreatedAt = artifact.CreatedAt;
            existing.ExpiresAt = artifact.ExpiresAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<TranscriptArtifactMetadata?> GetByIdAsync(Guid artifactId, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.TranscriptArtifacts
            .AsNoTracking()
            .SingleOrDefaultAsync(artifact => artifact.Id == artifactId, cancellationToken);

        return record is null ? null : MeetingAgentPersistenceMapper.ToDomain(record);
    }
}
