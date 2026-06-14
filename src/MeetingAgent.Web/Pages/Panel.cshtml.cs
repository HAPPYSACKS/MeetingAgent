using MeetingAgent.Application.Alerts;
using MeetingAgent.Application.Pacing;
using MeetingAgent.Application.Storage;
using MeetingAgent.Domain.Entities;
using MeetingAgent.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MeetingAgent.Web.Pages;

public class PanelModel(
    IMeetingSessionRepository meetingSessionRepository,
    IAgendaPlanRepository agendaPlanRepository,
    IFacilitatorAlertRepository alertRepository,
    IPacingAlertService pacingAlertService,
    IPacingEngine pacingEngine) : PageModel
{
    public MeetingSession? Meeting { get; private set; }

    public AgendaPlan? Agenda { get; private set; }

    public IReadOnlyList<PanelSection> Sections { get; private set; } = [];

    public IReadOnlyList<PanelAlert> Alerts { get; private set; } = [];

    public string HostStatus { get; private set; } = "Missing agenda";

    public string PacingRisk { get; private set; } = "Unknown";

    public string SuggestedIntervention { get; private set; } = "Create and approve an agenda before using the side panel.";

    public int ElapsedMinutes { get; private set; }

    public int RemainingMinutes { get; private set; }

    public bool HasApprovedAgenda => Meeting is not null && Agenda is not null;

    public async Task OnGetAsync([FromQuery] Guid? meetingId, CancellationToken cancellationToken)
    {
        await LoadPanelAsync(meetingId, true, cancellationToken);
    }

    public async Task<IActionResult> OnPostDismissAlertAsync(Guid meetingId, Guid alertId, CancellationToken cancellationToken)
    {
        var alert = await alertRepository.GetByIdAsync(alertId, cancellationToken);
        if (alert is not null && alert.MeetingId == meetingId)
        {
            alert.Dismiss();
            await alertRepository.SaveAsync(alert, cancellationToken);
        }

        return RedirectToPage(new { meetingId });
    }

    public async Task<IActionResult> OnPostResolveAlertAsync(Guid meetingId, Guid alertId, CancellationToken cancellationToken)
    {
        var alert = await alertRepository.GetByIdAsync(alertId, cancellationToken);
        if (alert is not null && alert.MeetingId == meetingId)
        {
            alert.Resolve();
            await alertRepository.SaveAsync(alert, cancellationToken);
        }

        return RedirectToPage(new { meetingId });
    }

    private async Task LoadPanelAsync(Guid? meetingId, bool syncAlerts, CancellationToken cancellationToken)
    {
        if (meetingId is null || meetingId == Guid.Empty)
        {
            return;
        }

        Meeting = await meetingSessionRepository.GetByIdAsync(meetingId.Value, cancellationToken);
        if (Meeting is null)
        {
            HostStatus = "Meeting not found";
            SuggestedIntervention = "Return to setup and approve an agenda for this meeting.";
            return;
        }

        Agenda = await agendaPlanRepository.GetLatestApprovedAsync(Meeting.Id, cancellationToken);
        if (Agenda is null)
        {
            HostStatus = "Agenda missing";
            SuggestedIntervention = "Open host setup and approve the agenda before relying on pacing guidance.";
            return;
        }

        var snapshot = pacingEngine.Calculate(Meeting, Agenda, DateTimeOffset.UtcNow);
        HostStatus = FormatMeetingStatus(snapshot.HostStatus);
        PacingRisk = FormatRisk(snapshot.RiskLevel);
        SuggestedIntervention = snapshot.SuggestedIntervention;
        ElapsedMinutes = snapshot.ElapsedMinutes;
        RemainingMinutes = snapshot.RemainingMinutes;
        Sections = snapshot.Sections
            .Select(section => new PanelSection(
                section.Title,
                section.Purpose,
                section.FacilitationNotes,
                section.StartsAtMinute,
                section.EndsAtMinute,
                section.IsCurrent))
            .ToArray();

        var alerts = syncAlerts
            ? await pacingAlertService.SyncPacingAlertsAsync(Meeting.Id, snapshot, DateTimeOffset.UtcNow, cancellationToken)
            : await alertRepository.ListForMeetingAsync(Meeting.Id, cancellationToken);
        Alerts = alerts
            .Where(alert => !alert.IsDismissed && !alert.IsResolved)
            .Select(alert => new PanelAlert(
                alert.Id,
                alert.Severity.ToString(),
                alert.EvidenceSnippet,
                alert.Recommendation))
            .ToArray();
    }

    private static string FormatMeetingStatus(MeetingStatus status) => status switch
    {
        MeetingStatus.Scheduled => "Meeting not started",
        MeetingStatus.InProgress => "Meeting in progress",
        MeetingStatus.Completed => "Meeting ended",
        _ => status.ToString()
    };

    private static string FormatRisk(PacingRiskLevel riskLevel) => riskLevel switch
    {
        PacingRiskLevel.PastEnd => "Past scheduled end",
        _ => riskLevel.ToString()
    };

    public sealed record PanelSection(
        string Title,
        string Purpose,
        string? FacilitationNotes,
        int StartsAtMinute,
        int EndsAtMinute,
        bool IsCurrent);

    public sealed record PanelAlert(
        Guid Id,
        string Severity,
        string Evidence,
        string Recommendation);
}
