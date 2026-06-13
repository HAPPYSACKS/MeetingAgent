namespace MeetingAgent.Application.Security;

public interface ICurrentUserContext
{
    bool TryGetCurrentUser(out AuthenticatedUser user);

    AuthenticatedUser GetRequiredCurrentUser();
}
