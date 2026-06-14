using MeetingAgent.Domain.Entities;

namespace MeetingAgent.Application.Storage;

public interface IFacilitatorAlertRepository
{
    Task SaveAsync(FacilitatorAlert alert, CancellationToken cancellationToken = default);

    Task<FacilitatorAlert?> GetByIdAsync(Guid alertId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FacilitatorAlert>> ListForMeetingAsync(Guid meetingId, CancellationToken cancellationToken = default);
}
