using MeetingAgent.Domain.Enums;

namespace MeetingAgent.Application.Pacing;

public sealed record MeetingPacingSnapshot(
    MeetingStatus HostStatus,
    int ElapsedMinutes,
    int RemainingMinutes,
    PacingRiskLevel RiskLevel,
    string SuggestedIntervention,
    IReadOnlyList<PacingAgendaSection> Sections);

public sealed record PacingAgendaSection(
    string Title,
    string Purpose,
    string? FacilitationNotes,
    int StartsAtMinute,
    int EndsAtMinute,
    bool IsCurrent);
