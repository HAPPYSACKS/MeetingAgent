using MeetingAgent.Application.Pacing;
using MeetingAgent.Domain.Entities;

namespace MeetingAgent.Application.Alerts;

public interface IPacingAlertService
{
    Task<IReadOnlyList<FacilitatorAlert>> SyncPacingAlertsAsync(
        Guid meetingId,
        MeetingPacingSnapshot snapshot,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);
}
