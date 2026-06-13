using MeetingAgent.Application.Security;
using Microsoft.Extensions.Logging;

namespace MeetingAgent.Infrastructure.Security;

public sealed class StructuredAuditLogger(ILogger<StructuredAuditLogger> logger) : IAuditLogger
{
    public void LogAgendaApproved(Guid meetingId, AuthenticatedUser actor)
    {
        logger.LogInformation(
            new EventId(AuditEventIds.AgendaApproved, nameof(AuditEventIds.AgendaApproved)),
            "Agenda approved for meeting {MeetingId} by actor {ActorObjectId} in tenant {TenantId}.",
            meetingId,
            actor.ObjectId,
            actor.TenantId);
    }

    public void LogRecapAccessed(Guid meetingId, AuthenticatedUser actor)
    {
        logger.LogInformation(
            new EventId(AuditEventIds.RecapAccessed, nameof(AuditEventIds.RecapAccessed)),
            "Meeting recap accessed for meeting {MeetingId} by actor {ActorObjectId} in tenant {TenantId}.",
            meetingId,
            actor.ObjectId,
            actor.TenantId);
    }

    public void LogUnauthorizedMeetingAccess(Guid meetingId, AuthenticatedUser? actor, string reason)
    {
        logger.LogWarning(
            new EventId(AuditEventIds.UnauthorizedMeetingAccess, nameof(AuditEventIds.UnauthorizedMeetingAccess)),
            "Unauthorized meeting access for meeting {MeetingId} by actor {ActorObjectId} in tenant {TenantId}: {Reason}",
            meetingId,
            actor?.ObjectId,
            actor?.TenantId,
            reason);
    }

    public void LogTeamsContextValidation(Guid meetingId, AuthenticatedUser actor, TeamsContextValidationResult result)
    {
        logger.LogInformation(
            new EventId(AuditEventIds.TeamsContextValidation, nameof(AuditEventIds.TeamsContextValidation)),
            "Teams context validation for meeting {MeetingId} by actor {ActorObjectId}: valid={IsValid}, reason={FailureReason}.",
            meetingId,
            actor.ObjectId,
            result.IsValid,
            result.FailureReason);
    }
}
