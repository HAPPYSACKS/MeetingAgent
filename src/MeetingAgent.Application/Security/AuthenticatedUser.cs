namespace MeetingAgent.Application.Security;

public sealed record AuthenticatedUser(
    string TenantId,
    string ObjectId,
    string? UserPrincipalName,
    string? DisplayName,
    string AuthenticationMode)
{
    public string HostIdentity => string.IsNullOrWhiteSpace(UserPrincipalName) ? ObjectId : UserPrincipalName;
}
