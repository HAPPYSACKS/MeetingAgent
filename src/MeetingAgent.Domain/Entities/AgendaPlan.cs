using MeetingAgent.Domain.Enums;
using MeetingAgent.Domain.Validation;

namespace MeetingAgent.Domain.Entities;

public sealed class AgendaPlan
{
    private readonly IReadOnlyList<AgendaSection> _agendaSections;

    public AgendaPlan(
        Guid id,
        Guid meetingId,
        string objective,
        TimeSpan totalDuration,
        IEnumerable<AgendaSection> agendaSections,
        int version,
        AgendaApprovalState approvalState,
        string? approvedBy = null,
        DateTimeOffset? approvedAt = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Agenda plan id is required.", nameof(id));
        }

        if (meetingId == Guid.Empty)
        {
            throw new ArgumentException("Meeting id is required.", nameof(meetingId));
        }

        if (version <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(version), version, "Agenda version must be greater than zero.");
        }

        DomainRules.EnsureEnumValue(approvalState, nameof(approvalState));
        DomainRules.EnsureMeetingDuration(totalDuration, nameof(totalDuration));

        var sections = (agendaSections ?? throw new ArgumentNullException(nameof(agendaSections))).ToArray();

        if (sections.Length == 0)
        {
            throw new ArgumentException("Agenda plan must contain at least one section.", nameof(agendaSections));
        }

        var totalSectionDuration = sections.Aggregate(TimeSpan.Zero, (total, section) => total + section.SuggestedDuration);
        if (totalSectionDuration > totalDuration)
        {
            throw new ArgumentException("Agenda section durations cannot exceed the total meeting duration.", nameof(agendaSections));
        }

        if (sections.Select(section => section.Order).Distinct().Count() != sections.Length)
        {
            throw new ArgumentException("Agenda section order values must be unique.", nameof(agendaSections));
        }

        if (approvalState == AgendaApprovalState.Approved)
        {
            if (string.IsNullOrWhiteSpace(approvedBy))
            {
                throw new ArgumentException("Approved agendas must record the approving host.", nameof(approvedBy));
            }

            if (approvedAt is null)
            {
                throw new ArgumentException("Approved agendas must record an approval timestamp.", nameof(approvedAt));
            }
        }

        Id = id;
        MeetingId = meetingId;
        Objective = DomainRules.Required(objective, nameof(objective));
        TotalDuration = totalDuration;
        _agendaSections = Array.AsReadOnly(sections.OrderBy(section => section.Order).ToArray());
        Version = version;
        ApprovalState = approvalState;
        ApprovedBy = string.IsNullOrWhiteSpace(approvedBy) ? null : approvedBy.Trim();
        ApprovedAt = approvedAt;
    }

    public Guid Id { get; }

    public Guid MeetingId { get; }

    public string Objective { get; }

    public TimeSpan TotalDuration { get; }

    public IReadOnlyList<AgendaSection> AgendaSections => _agendaSections;

    public int Version { get; }

    public AgendaApprovalState ApprovalState { get; private set; }

    public string? ApprovedBy { get; private set; }

    public DateTimeOffset? ApprovedAt { get; private set; }

    public void Approve(MeetingSession meetingSession, string hostIdentity, DateTimeOffset approvedAt)
    {
        ArgumentNullException.ThrowIfNull(meetingSession);

        if (meetingSession.Id != MeetingId)
        {
            throw new InvalidOperationException("Agenda plan does not belong to the provided meeting session.");
        }

        meetingSession.AssertHostOwnership(hostIdentity);

        ApprovalState = AgendaApprovalState.Approved;
        ApprovedBy = DomainRules.Required(hostIdentity, nameof(hostIdentity));
        ApprovedAt = approvedAt;
    }
}
