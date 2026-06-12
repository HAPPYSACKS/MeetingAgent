using MeetingAgent.Application.Storage;
using MeetingAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeetingAgent.Infrastructure.Persistence.Repositories;

public sealed class MeetingRecapRepository(MeetingAgentDbContext dbContext) : IMeetingRecapRepository
{
    public async Task SaveAsync(MeetingRecap recap, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recap);

        var existing = await dbContext.MeetingRecaps
            .Include(record => record.Insights)
            .Include(record => record.ActionItems)
            .SingleOrDefaultAsync(record => record.Id == recap.Id, cancellationToken);

        if (existing is null)
        {
            dbContext.MeetingRecaps.Add(MeetingAgentPersistenceMapper.ToRecord(recap));
        }
        else
        {
            dbContext.RecapInsights.RemoveRange(existing.Insights);
            dbContext.RecapActionItems.RemoveRange(existing.ActionItems);
            MeetingAgentPersistenceMapper.CopyToRecord(recap, existing);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MeetingRecap?> GetByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.MeetingRecaps
            .AsNoTracking()
            .Include(recap => recap.Insights)
            .Include(recap => recap.ActionItems)
            .Where(recap => recap.MeetingId == meetingId)
            .OrderByDescending(recap => recap.GeneratedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return record is null ? null : MeetingAgentPersistenceMapper.ToDomain(record);
    }
}
