using MeetingAgent.Application.Storage;
using MeetingAgent.Domain.Entities;
using MeetingAgent.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MeetingAgent.Infrastructure.Persistence.Repositories;

public sealed class AgendaPlanRepository(MeetingAgentDbContext dbContext) : IAgendaPlanRepository
{
    public async Task SaveAsync(AgendaPlan agendaPlan, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(agendaPlan);

        var existing = await dbContext.AgendaPlans
            .Include(plan => plan.AgendaSections)
            .SingleOrDefaultAsync(plan => plan.Id == agendaPlan.Id, cancellationToken);

        if (existing is null)
        {
            dbContext.AgendaPlans.Add(MeetingAgentPersistenceMapper.ToRecord(agendaPlan));
        }
        else
        {
            dbContext.AgendaSections.RemoveRange(existing.AgendaSections);
            MeetingAgentPersistenceMapper.CopyToRecord(agendaPlan, existing);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AgendaPlan>> ListVersionsAsync(Guid meetingId, CancellationToken cancellationToken = default)
    {
        var records = await dbContext.AgendaPlans
            .AsNoTracking()
            .Include(plan => plan.AgendaSections)
            .Where(plan => plan.MeetingId == meetingId)
            .OrderBy(plan => plan.Version)
            .ToListAsync(cancellationToken);

        return records.Select(MeetingAgentPersistenceMapper.ToDomain).ToArray();
    }

    public async Task<AgendaPlan?> GetLatestApprovedAsync(Guid meetingId, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.AgendaPlans
            .AsNoTracking()
            .Include(plan => plan.AgendaSections)
            .Where(plan => plan.MeetingId == meetingId && plan.ApprovalState == AgendaApprovalState.Approved)
            .OrderByDescending(plan => plan.Version)
            .FirstOrDefaultAsync(cancellationToken);

        return record is null ? null : MeetingAgentPersistenceMapper.ToDomain(record);
    }
}
