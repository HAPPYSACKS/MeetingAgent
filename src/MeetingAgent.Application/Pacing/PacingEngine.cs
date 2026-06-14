using MeetingAgent.Domain.Entities;
using MeetingAgent.Domain.Enums;

namespace MeetingAgent.Application.Pacing;

public sealed class PacingEngine : IPacingEngine
{
    public MeetingPacingSnapshot Calculate(MeetingSession meetingSession, AgendaPlan agendaPlan, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(meetingSession);
        ArgumentNullException.ThrowIfNull(agendaPlan);

        var elapsed = now <= meetingSession.ScheduledStart
            ? TimeSpan.Zero
            : now - meetingSession.ScheduledStart;
        var remaining = meetingSession.ScheduledEnd <= now
            ? TimeSpan.Zero
            : meetingSession.ScheduledEnd - now;

        var elapsedMinutes = Math.Max(0, (int)Math.Floor(elapsed.TotalMinutes));
        var remainingMinutes = Math.Max(0, (int)Math.Ceiling(remaining.TotalMinutes));
        var hostStatus = GetHostStatus(meetingSession, now);
        var sections = BuildSections(agendaPlan, elapsedMinutes);
        var riskLevel = CalculateRisk(meetingSession, agendaPlan, now, elapsed, remainingMinutes);
        var suggestedIntervention = SuggestIntervention(riskLevel);

        return new MeetingPacingSnapshot(
            hostStatus,
            elapsedMinutes,
            remainingMinutes,
            riskLevel,
            suggestedIntervention,
            sections);
    }

    private static MeetingStatus GetHostStatus(MeetingSession meetingSession, DateTimeOffset now)
    {
        if (now < meetingSession.ScheduledStart)
        {
            return MeetingStatus.Scheduled;
        }

        return now > meetingSession.ScheduledEnd
            ? MeetingStatus.Completed
            : MeetingStatus.InProgress;
    }

    private static IReadOnlyList<PacingAgendaSection> BuildSections(AgendaPlan agendaPlan, int elapsedMinutes)
    {
        var sectionStart = 0;
        var sections = new List<PacingAgendaSection>();

        foreach (var section in agendaPlan.AgendaSections)
        {
            var sectionDuration = (int)Math.Ceiling(section.SuggestedDuration.TotalMinutes);
            var sectionEnd = sectionStart + sectionDuration;
            sections.Add(new PacingAgendaSection(
                section.Title,
                section.Purpose,
                section.FacilitationNotes,
                sectionStart,
                sectionEnd,
                elapsedMinutes >= sectionStart && elapsedMinutes < sectionEnd));
            sectionStart = sectionEnd;
        }

        return sections;
    }

    private static PacingRiskLevel CalculateRisk(
        MeetingSession meetingSession,
        AgendaPlan agendaPlan,
        DateTimeOffset now,
        TimeSpan elapsed,
        int remainingMinutes)
    {
        if (now > meetingSession.ScheduledEnd)
        {
            return PacingRiskLevel.PastEnd;
        }

        var percentElapsed = meetingSession.ScheduledDuration.TotalMinutes <= 0
            ? 0
            : elapsed.TotalMinutes / meetingSession.ScheduledDuration.TotalMinutes;
        var percentAgendaPlanned = agendaPlan.TotalDuration.TotalMinutes <= 0
            ? 0
            : Math.Min(1, elapsed.TotalMinutes / agendaPlan.TotalDuration.TotalMinutes);

        if (percentElapsed >= 0.85 && remainingMinutes <= 10)
        {
            return PacingRiskLevel.High;
        }

        return percentElapsed - percentAgendaPlanned > 0.15
            ? PacingRiskLevel.Medium
            : PacingRiskLevel.Low;
    }

    private static string SuggestIntervention(PacingRiskLevel riskLevel)
    {
        return riskLevel switch
        {
            PacingRiskLevel.PastEnd => "Move to a crisp close: decisions, owners, and what happens next.",
            PacingRiskLevel.High => "Protect the close. Park new topics and ask for final commitments.",
            PacingRiskLevel.Medium => "Name the drift and ask whether to compress, park, or skip the next section.",
            _ => "Stay with the current section and keep an eye on the next transition."
        };
    }
}
