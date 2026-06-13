namespace MeetingAgent.Application.Security;

public sealed record TeamsContextValidationResult(
    bool IsValid,
    string? FailureReason,
    string? ProvidedTeamsMeetingId,
    string? ProvidedTeamsChatId);
