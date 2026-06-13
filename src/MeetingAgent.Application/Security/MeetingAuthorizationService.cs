using MeetingAgent.Application.Storage;

namespace MeetingAgent.Application.Security;

public sealed class MeetingAuthorizationService(
    ICurrentUserContext currentUserContext,
    IMeetingSessionRepository meetingSessionRepository,
    IAuditLogger auditLogger) : IMeetingAuthorizationService
{
    public async Task<MeetingAuthorizationResult> AuthorizeHostAccessAsync(
        Guid meetingId,
        TeamsMeetingContextSelector? teamsContextSelector = null,
        CancellationToken cancellationToken = default)
    {
        if (!currentUserContext.TryGetCurrentUser(out var user))
        {
            auditLogger.LogUnauthorizedMeetingAccess(meetingId, null, "Missing authenticated user identity.");
            return MeetingAuthorizationResult.Denied("Missing authenticated user identity.");
        }

        var meetingSession = await meetingSessionRepository.GetByIdAsync(meetingId, cancellationToken);
        if (meetingSession is null)
        {
            auditLogger.LogUnauthorizedMeetingAccess(meetingId, user, "Meeting session was not found.");
            return MeetingAuthorizationResult.Denied("Meeting session was not found.", user);
        }

        var teamsContextValidation = TeamsMeetingContextValidator.Validate(meetingSession, teamsContextSelector);
        auditLogger.LogTeamsContextValidation(meetingId, user, teamsContextValidation);
        if (!teamsContextValidation.IsValid)
        {
            auditLogger.LogUnauthorizedMeetingAccess(meetingId, user, teamsContextValidation.FailureReason!);
            return MeetingAuthorizationResult.Denied(teamsContextValidation.FailureReason!, user, meetingSession);
        }

        try
        {
            meetingSession.AssertHostOwnership(user.HostIdentity);
        }
        catch (InvalidOperationException ex)
        {
            auditLogger.LogUnauthorizedMeetingAccess(meetingId, user, "Authenticated user is not the meeting organizer.");
            return MeetingAuthorizationResult.Denied(ex.Message, user, meetingSession);
        }

        return MeetingAuthorizationResult.Authorized(meetingSession, user);
    }
}
