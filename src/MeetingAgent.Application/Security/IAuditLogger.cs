namespace MeetingAgent.Application.Security;

public interface IAuditLogger
{
    void LogAgendaApproved(Guid meetingId, AuthenticatedUser actor);

    void LogRecapAccessed(Guid meetingId, AuthenticatedUser actor);

    void LogUnauthorizedMeetingAccess(Guid meetingId, AuthenticatedUser? actor, string reason);

    void LogTeamsContextValidation(Guid meetingId, AuthenticatedUser actor, TeamsContextValidationResult result);
}
