using MeetingAgent.Application.Storage;
using MeetingAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeetingAgent.Infrastructure.Persistence.Repositories;

public sealed class MeetingSessionRepository(MeetingAgentDbContext dbContext) : IMeetingSessionRepository
{
    public async Task SaveAsync(MeetingSession meetingSession, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(meetingSession);

        var existing = await dbContext.MeetingSessions
            .SingleOrDefaultAsync(meeting => meeting.Id == meetingSession.Id, cancellationToken);

        if (existing is null)
        {
            dbContext.MeetingSessions.Add(MeetingAgentPersistenceMapper.ToRecord(meetingSession));
        }
        else
        {
            MeetingAgentPersistenceMapper.CopyToRecord(meetingSession, existing);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MeetingSession?> GetByIdAsync(Guid meetingId, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.MeetingSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(meeting => meeting.Id == meetingId, cancellationToken);

        return record is null ? null : MeetingAgentPersistenceMapper.ToDomain(record);
    }
}
