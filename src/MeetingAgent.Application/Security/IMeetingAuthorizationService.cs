namespace MeetingAgent.Application.Security;

public interface IMeetingAuthorizationService
{
    Task<MeetingAuthorizationResult> AuthorizeHostAccessAsync(
        Guid meetingId,
        TeamsMeetingContextSelector? teamsContextSelector = null,
        CancellationToken cancellationToken = default);
}
