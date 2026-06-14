using FluentAssertions;
using MeetingAgent.Application.Alerts;
using MeetingAgent.Application.Pacing;
using MeetingAgent.Application.Storage;
using MeetingAgent.Domain.Entities;
using MeetingAgent.Domain.Enums;

namespace MeetingAgent.UnitTests;

public sealed class PacingAlertServiceTests
{
    [Fact]
    public async Task SyncPacingAlertsAsync_CreatesAlertForHighRiskSnapshot()
    {
        var repository = new InMemoryAlertRepository();
        var service = new PacingAlertService(repository);
        var meetingId = Guid.NewGuid();

        var alerts = await service.SyncPacingAlertsAsync(
            meetingId,
            CreateSnapshot(PacingRiskLevel.High),
            DateTimeOffset.UtcNow);

        alerts.Should().ContainSingle();
        alerts[0].Severity.Should().Be(AlertSeverity.High);
        alerts[0].Recommendation.Should().Contain("Protect the close");
    }

    [Fact]
    public async Task SyncPacingAlertsAsync_DoesNotCreateDuplicateActiveAlert()
    {
        var repository = new InMemoryAlertRepository();
        var service = new PacingAlertService(repository);
        var meetingId = Guid.NewGuid();
        var snapshot = CreateSnapshot(PacingRiskLevel.High);

        await service.SyncPacingAlertsAsync(meetingId, snapshot, DateTimeOffset.UtcNow);
        await service.SyncPacingAlertsAsync(meetingId, snapshot, DateTimeOffset.UtcNow.AddMinutes(1));

        var alerts = await repository.ListForMeetingAsync(meetingId);
        alerts.Should().ContainSingle();
    }

    [Fact]
    public async Task SyncPacingAlertsAsync_ExcludesDismissedAlertFromActiveList()
    {
        var repository = new InMemoryAlertRepository();
        var service = new PacingAlertService(repository);
        var meetingId = Guid.NewGuid();

        var alerts = await service.SyncPacingAlertsAsync(meetingId, CreateSnapshot(PacingRiskLevel.High), DateTimeOffset.UtcNow);
        alerts[0].Dismiss();
        await repository.SaveAsync(alerts[0]);

        var activeAlerts = await service.SyncPacingAlertsAsync(meetingId, CreateSnapshot(PacingRiskLevel.Low), DateTimeOffset.UtcNow);

        activeAlerts.Should().BeEmpty();
    }

    private static MeetingPacingSnapshot CreateSnapshot(PacingRiskLevel riskLevel)
    {
        var intervention = riskLevel switch
        {
            PacingRiskLevel.High => "Protect the close. Park new topics and ask for final commitments.",
            PacingRiskLevel.Medium => "Name the drift and ask whether to compress, park, or skip the next section.",
            PacingRiskLevel.PastEnd => "Move to a crisp close: decisions, owners, and what happens next.",
            _ => "Stay with the current section and keep an eye on the next transition."
        };

        return new MeetingPacingSnapshot(
            MeetingStatus.InProgress,
            25,
            5,
            riskLevel,
            intervention,
            []);
    }

    private sealed class InMemoryAlertRepository : IFacilitatorAlertRepository
    {
        private readonly List<FacilitatorAlert> _alerts = [];

        public Task SaveAsync(FacilitatorAlert alert, CancellationToken cancellationToken = default)
        {
            _alerts.RemoveAll(existing => existing.Id == alert.Id);
            _alerts.Add(alert);
            return Task.CompletedTask;
        }

        public Task<FacilitatorAlert?> GetByIdAsync(Guid alertId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_alerts.SingleOrDefault(alert => alert.Id == alertId));
        }

        public Task<IReadOnlyList<FacilitatorAlert>> ListForMeetingAsync(Guid meetingId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<FacilitatorAlert>>(_alerts.Where(alert => alert.MeetingId == meetingId).ToArray());
        }
    }
}
