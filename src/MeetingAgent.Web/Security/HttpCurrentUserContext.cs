using MeetingAgent.Application.Security;

namespace MeetingAgent.Web.Security;

public sealed class HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public bool TryGetCurrentUser(out AuthenticatedUser user)
    {
        user = null!;

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return false;
        }

        return CurrentUserClaimsReader.TryRead(
            httpContext.User,
            ResolveAuthenticationMode(httpContext),
            out user);
    }

    public AuthenticatedUser GetRequiredCurrentUser()
    {
        if (TryGetCurrentUser(out var user))
        {
            return user;
        }

        throw new InvalidOperationException("The current request does not contain a valid authenticated user.");
    }

    private static string ResolveAuthenticationMode(HttpContext httpContext)
    {
        var authorizationHeader = httpContext.Request.Headers.Authorization.ToString();
        if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return "Bearer";
        }

        return httpContext.User.Identity?.AuthenticationType ?? "Cookie";
    }
}
