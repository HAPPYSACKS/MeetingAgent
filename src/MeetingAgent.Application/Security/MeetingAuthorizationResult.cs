using MeetingAgent.Domain.Entities;

namespace MeetingAgent.Application.Security;

public sealed record MeetingAuthorizationResult(
    bool IsAuthorized,
    MeetingSession? MeetingSession,
    AuthenticatedUser? User,
    string? FailureReason)
{
    public static MeetingAuthorizationResult Authorized(MeetingSession meetingSession, AuthenticatedUser user)
    {
        return new MeetingAuthorizationResult(true, meetingSession, user, null);
    }

    public static MeetingAuthorizationResult Denied(
        string failureReason,
        AuthenticatedUser? user = null,
        MeetingSession? meetingSession = null)
    {
        return new MeetingAuthorizationResult(false, meetingSession, user, failureReason);
    }
}
