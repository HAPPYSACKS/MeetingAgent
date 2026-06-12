using MeetingAgent.Application.Storage;
using MeetingAgent.Domain.Entities;
using MeetingAgent.Domain.Enums;
using MeetingAgent.Domain.ValueObjects;
using MeetingAgent.Infrastructure.Persistence.Records;

namespace MeetingAgent.Infrastructure.Persistence;

internal static class MeetingAgentPersistenceMapper
{
    public static MeetingSessionRecord ToRecord(MeetingSession meetingSession)
    {
        var record = new MeetingSessionRecord();
        CopyToRecord(meetingSession, record);
        return record;
    }

    public static void CopyToRecord(MeetingSession meetingSession, MeetingSessionRecord record)
    {
        record.Id = meetingSession.Id;
        record.OrganizerIdentity = meetingSession.OrganizerIdentity;
        record.Title = meetingSession.Title;
        record.ScheduledStart = meetingSession.ScheduledStart;
        record.ScheduledEnd = meetingSession.ScheduledEnd;
        record.Status = meetingSession.Status;
        record.TeamsMeetingId = meetingSession.TeamsContextIdentifiers.TeamsMeetingId;
        record.TeamsChatId = meetingSession.TeamsContextIdentifiers.TeamsChatId;
        record.CalendarEventId = meetingSession.TeamsContextIdentifiers.CalendarEventId;
    }

    public static MeetingSession ToDomain(MeetingSessionRecord record)
    {
        return new MeetingSession(
            record.Id,
            record.OrganizerIdentity,
            record.Title,
            record.ScheduledStart,
            record.ScheduledEnd,
            record.Status,
            new TeamsMeetingContextIdentifiers(record.TeamsMeetingId, record.TeamsChatId, record.CalendarEventId));
    }

    public static AgendaPlanRecord ToRecord(AgendaPlan agendaPlan)
    {
        var record = new AgendaPlanRecord();
        CopyToRecord(agendaPlan, record);
        return record;
    }

    public static void CopyToRecord(AgendaPlan agendaPlan, AgendaPlanRecord record)
    {
        record.Id = agendaPlan.Id;
        record.MeetingId = agendaPlan.MeetingId;
        record.Objective = agendaPlan.Objective;
        record.TotalDuration = agendaPlan.TotalDuration;
        record.Version = agendaPlan.Version;
        record.ApprovalState = agendaPlan.ApprovalState;
        record.ApprovedBy = agendaPlan.ApprovedBy;
        record.ApprovedAt = agendaPlan.ApprovedAt;
        record.AgendaSections.Clear();
        record.AgendaSections.AddRange(agendaPlan.AgendaSections.Select(section => new AgendaSectionRecord
        {
            AgendaPlanId = agendaPlan.Id,
            Title = section.Title,
            Purpose = section.Purpose,
            SuggestedDuration = section.SuggestedDuration,
            Order = section.Order,
            FacilitationNotes = section.FacilitationNotes
        }));
    }

    public static AgendaPlan ToDomain(AgendaPlanRecord record)
    {
        return new AgendaPlan(
            record.Id,
            record.MeetingId,
            record.Objective,
            record.TotalDuration,
            record.AgendaSections
                .OrderBy(section => section.Order)
                .Select(section => new AgendaSection(
                    section.Title,
                    section.Purpose,
                    section.SuggestedDuration,
                    section.Order,
                    section.FacilitationNotes)),
            record.Version,
            record.ApprovalState,
            record.ApprovedBy,
            record.ApprovedAt);
    }

    public static FacilitatorAlertRecord ToRecord(FacilitatorAlert alert)
    {
        var record = new FacilitatorAlertRecord();
        CopyToRecord(alert, record);
        return record;
    }

    public static void CopyToRecord(FacilitatorAlert alert, FacilitatorAlertRecord record)
    {
        record.Id = alert.Id;
        record.MeetingId = alert.MeetingId;
        record.AlertType = alert.AlertType;
        record.Severity = alert.Severity;
        record.Timestamp = alert.Timestamp;
        record.Source = alert.Source;
        record.EvidenceSnippet = alert.EvidenceSnippet;
        record.Recommendation = alert.Recommendation;
        record.IsDismissed = alert.IsDismissed;
        record.IsResolved = alert.IsResolved;
    }

    public static FacilitatorAlert ToDomain(FacilitatorAlertRecord record)
    {
        return new FacilitatorAlert(
            record.Id,
            record.MeetingId,
            record.AlertType,
            record.Severity,
            record.Timestamp,
            record.Source,
            record.EvidenceSnippet,
            record.Recommendation,
            record.IsDismissed,
            record.IsResolved);
    }

    public static MeetingRecapRecord ToRecord(MeetingRecap recap)
    {
        var record = new MeetingRecapRecord();
        CopyToRecord(recap, record);
        return record;
    }

    public static void CopyToRecord(MeetingRecap recap, MeetingRecapRecord record)
    {
        record.Id = recap.Id;
        record.MeetingId = recap.MeetingId;
        record.PacingSummary = recap.PacingSummary;
        record.GeneratedAt = recap.GeneratedAt;
        record.TranscriptAvailability = recap.TranscriptAvailability;
        record.Status = recap.Status;
        record.Insights.Clear();
        record.ActionItems.Clear();

        record.Insights.AddRange(recap.TopicDriftMoments.Select(insight => new RecapInsightRecord
        {
            MeetingRecapId = recap.Id,
            Kind = RecapInsightKind.TopicDrift,
            Summary = insight.Summary,
            EvidenceSnippet = insight.EvidenceSnippet
        }));
        record.Insights.AddRange(recap.ConfusionMoments.Select(insight => new RecapInsightRecord
        {
            MeetingRecapId = recap.Id,
            Kind = RecapInsightKind.Confusion,
            Summary = insight.Summary,
            EvidenceSnippet = insight.EvidenceSnippet
        }));
        record.ActionItems.AddRange(recap.ActionItems.Select(actionItem => new RecapActionItemRecord
        {
            MeetingRecapId = recap.Id,
            Description = actionItem.Description,
            Owner = actionItem.Owner
        }));
    }

    public static MeetingRecap ToDomain(MeetingRecapRecord record)
    {
        return new MeetingRecap(
            record.Id,
            record.MeetingId,
            record.PacingSummary,
            record.Insights
                .Where(insight => insight.Kind == RecapInsightKind.TopicDrift)
                .OrderBy(insight => insight.Id)
                .Select(insight => new RecapInsight(insight.Summary, insight.EvidenceSnippet)),
            record.Insights
                .Where(insight => insight.Kind == RecapInsightKind.Confusion)
                .OrderBy(insight => insight.Id)
                .Select(insight => new RecapInsight(insight.Summary, insight.EvidenceSnippet)),
            record.ActionItems
                .OrderBy(actionItem => actionItem.Id)
                .Select(actionItem => new RecapActionItem(actionItem.Description, actionItem.Owner)),
            record.GeneratedAt,
            record.TranscriptAvailability,
            record.Status);
    }

    public static TranscriptArtifactRecord ToRecord(TranscriptArtifactMetadata artifact)
    {
        return new TranscriptArtifactRecord
        {
            Id = artifact.Id,
            MeetingId = artifact.MeetingId,
            BlobContainerName = artifact.BlobContainerName,
            BlobName = artifact.BlobName,
            CreatedAt = artifact.CreatedAt,
            ExpiresAt = artifact.ExpiresAt
        };
    }

    public static TranscriptArtifactMetadata ToDomain(TranscriptArtifactRecord record)
    {
        return new TranscriptArtifactMetadata(
            record.Id,
            record.MeetingId,
            record.BlobContainerName,
            record.BlobName,
            record.CreatedAt,
            record.ExpiresAt);
    }
}
