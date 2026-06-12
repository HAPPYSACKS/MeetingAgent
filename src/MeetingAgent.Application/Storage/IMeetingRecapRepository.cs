using MeetingAgent.Domain.Entities;

namespace MeetingAgent.Application.Storage;

public interface IMeetingRecapRepository
{
    Task SaveAsync(MeetingRecap recap, CancellationToken cancellationToken = default);

    Task<MeetingRecap?> GetByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken = default);
}
