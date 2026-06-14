using MeetingAgent.Application.Pacing;
using MeetingAgent.Application.Storage;
using MeetingAgent.Domain.Entities;
using MeetingAgent.Domain.Enums;

namespace MeetingAgent.Application.Alerts;

public sealed class PacingAlertService(IFacilitatorAlertRepository alertRepository) : IPacingAlertService
{
    public async Task<IReadOnlyList<FacilitatorAlert>> SyncPacingAlertsAsync(
        Guid meetingId,
        MeetingPacingSnapshot snapshot,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var alerts = await alertRepository.ListForMeetingAsync(meetingId, cancellationToken);
        if (snapshot.RiskLevel is PacingRiskLevel.Low)
        {
            return ActiveAlerts(alerts);
        }

        var severity = ToSeverity(snapshot.RiskLevel);
        var existing = alerts.FirstOrDefault(alert =>
            alert.AlertType == AlertType.PacingRisk
            && alert.Source == AlertSource.PacingEngine
            && alert.Severity == severity
            && !alert.IsDismissed
            && !alert.IsResolved
            && string.Equals(alert.Recommendation, snapshot.SuggestedIntervention, StringComparison.Ordinal));

        if (existing is null)
        {
            var alert = new FacilitatorAlert(
                Guid.NewGuid(),
                meetingId,
                AlertType.PacingRisk,
                severity,
                now,
                AlertSource.PacingEngine,
                BuildEvidence(snapshot),
                snapshot.SuggestedIntervention,
                false,
                false);

            await alertRepository.SaveAsync(alert, cancellationToken);
            alerts = await alertRepository.ListForMeetingAsync(meetingId, cancellationToken);
        }

        return ActiveAlerts(alerts);
    }

    private static IReadOnlyList<FacilitatorAlert> ActiveAlerts(IEnumerable<FacilitatorAlert> alerts)
    {
        return alerts
            .Where(alert => !alert.IsDismissed && !alert.IsResolved)
            .OrderByDescending(alert => alert.Timestamp)
            .ToArray();
    }

    private static AlertSeverity ToSeverity(PacingRiskLevel riskLevel) => riskLevel switch
    {
        PacingRiskLevel.Medium => AlertSeverity.Medium,
        PacingRiskLevel.High => AlertSeverity.High,
        PacingRiskLevel.PastEnd => AlertSeverity.Critical,
        _ => AlertSeverity.Low
    };

    private static string BuildEvidence(MeetingPacingSnapshot snapshot)
    {
        return snapshot.RiskLevel switch
        {
            PacingRiskLevel.PastEnd => $"The meeting is past its scheduled end with {snapshot.ElapsedMinutes} minutes elapsed.",
            PacingRiskLevel.High => $"The meeting is in its final stretch with {snapshot.RemainingMinutes} minutes remaining.",
            PacingRiskLevel.Medium => $"Pacing risk increased after {snapshot.ElapsedMinutes} elapsed minutes.",
            _ => "The meeting is currently on track."
        };
    }
}
