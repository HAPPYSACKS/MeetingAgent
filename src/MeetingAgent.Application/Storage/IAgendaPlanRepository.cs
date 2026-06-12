using MeetingAgent.Domain.Entities;

namespace MeetingAgent.Application.Storage;

public interface IAgendaPlanRepository
{
    Task SaveAsync(AgendaPlan agendaPlan, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AgendaPlan>> ListVersionsAsync(Guid meetingId, CancellationToken cancellationToken = default);

    Task<AgendaPlan?> GetLatestApprovedAsync(Guid meetingId, CancellationToken cancellationToken = default);
}
