using MeetingAgent.Domain.Entities;

namespace MeetingAgent.Application.Storage;

public interface IMeetingSessionRepository
{
    Task SaveAsync(MeetingSession meetingSession, CancellationToken cancellationToken = default);

    Task<MeetingSession?> GetByIdAsync(Guid meetingId, CancellationToken cancellationToken = default);
}
