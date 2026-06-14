using MeetingAgent.Application.Storage;
using MeetingAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeetingAgent.Infrastructure.Persistence.Repositories;

public sealed class FacilitatorAlertRepository(MeetingAgentDbContext dbContext) : IFacilitatorAlertRepository
{
    public async Task SaveAsync(FacilitatorAlert alert, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alert);

        var existing = await dbContext.FacilitatorAlerts
            .SingleOrDefaultAsync(record => record.Id == alert.Id, cancellationToken);

        if (existing is null)
        {
            dbContext.FacilitatorAlerts.Add(MeetingAgentPersistenceMapper.ToRecord(alert));
        }
        else
        {
            MeetingAgentPersistenceMapper.CopyToRecord(alert, existing);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FacilitatorAlert>> ListForMeetingAsync(Guid meetingId, CancellationToken cancellationToken = default)
    {
        var records = await dbContext.FacilitatorAlerts
            .AsNoTracking()
            .Where(alert => alert.MeetingId == meetingId)
            .OrderBy(alert => alert.Timestamp)
            .ToListAsync(cancellationToken);

        return records.Select(MeetingAgentPersistenceMapper.ToDomain).ToArray();
    }

    public async Task<FacilitatorAlert?> GetByIdAsync(Guid alertId, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.FacilitatorAlerts
            .AsNoTracking()
            .SingleOrDefaultAsync(alert => alert.Id == alertId, cancellationToken);

        return record is null ? null : MeetingAgentPersistenceMapper.ToDomain(record);
    }
}
