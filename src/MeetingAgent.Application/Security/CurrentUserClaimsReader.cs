using System.Security.Claims;

namespace MeetingAgent.Application.Security;

public static class CurrentUserClaimsReader
{
    private static readonly string[] TenantClaimTypes =
    [
        "tid",
        "http://schemas.microsoft.com/identity/claims/tenantid"
    ];

    private static readonly string[] ObjectIdClaimTypes =
    [
        "oid",
        "http://schemas.microsoft.com/identity/claims/objectidentifier"
    ];

    private static readonly string[] UserPrincipalNameClaimTypes =
    [
        "preferred_username",
        "upn",
        ClaimTypes.Upn,
        "email",
        ClaimTypes.Email
    ];

    private static readonly string[] DisplayNameClaimTypes =
    [
        "name",
        ClaimTypes.Name
    ];

    public static bool TryRead(ClaimsPrincipal principal, string authenticationMode, out AuthenticatedUser user)
    {
        ArgumentNullException.ThrowIfNull(principal);

        user = null!;

        if (principal.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var tenantId = FirstClaimValue(principal, TenantClaimTypes);
        var objectId = FirstClaimValue(principal, ObjectIdClaimTypes);

        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(objectId))
        {
            return false;
        }

        var userPrincipalName = FirstClaimValue(principal, UserPrincipalNameClaimTypes);
        var displayName = FirstClaimValue(principal, DisplayNameClaimTypes) ?? userPrincipalName;

        user = new AuthenticatedUser(
            tenantId.Trim(),
            objectId.Trim(),
            string.IsNullOrWhiteSpace(userPrincipalName) ? null : userPrincipalName.Trim(),
            string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim(),
            string.IsNullOrWhiteSpace(authenticationMode) ? "unknown" : authenticationMode.Trim());

        return true;
    }

    private static string? FirstClaimValue(ClaimsPrincipal principal, IEnumerable<string> claimTypes)
    {
        return claimTypes
            .Select(claimType => principal.FindFirst(claimType)?.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
