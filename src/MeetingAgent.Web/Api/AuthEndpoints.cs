using MeetingAgent.Application.Security;
using MeetingAgent.Web.Security;

namespace MeetingAgent.Web.Api;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthApi(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/auth")
            .WithTags("Authentication")
            .RequireAuthorization(MeetingAgentAuthentication.AuthenticatedPolicy);

        group.MapGet("/me", (ICurrentUserContext currentUserContext) =>
        {
            if (!currentUserContext.TryGetCurrentUser(out var user))
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new
            {
                tenantId = user.TenantId,
                objectId = user.ObjectId,
                userPrincipalName = user.UserPrincipalName,
                displayName = user.DisplayName,
                authenticationMode = user.AuthenticationMode
            });
        });

        group.MapGet(
            "/meetings/{meetingId:guid}/host-access",
            async (
                Guid meetingId,
                HttpRequest request,
                IMeetingAuthorizationService authorizationService,
                CancellationToken cancellationToken) =>
            {
                var selector = new TeamsMeetingContextSelector(
                    request.Headers["X-Teams-Meeting-Id"].FirstOrDefault(),
                    request.Headers["X-Teams-Chat-Id"].FirstOrDefault());

                var result = await authorizationService.AuthorizeHostAccessAsync(
                    meetingId,
                    selector,
                    cancellationToken);

                return result.IsAuthorized
                    ? Results.NoContent()
                    : Results.Forbid();
            });

        return endpoints;
    }
}
