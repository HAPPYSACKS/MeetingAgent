using MeetingAgent.Domain.Entities;

namespace MeetingAgent.Application.Security;

public static class TeamsMeetingContextValidator
{
    public static TeamsContextValidationResult Validate(
        MeetingSession meetingSession,
        TeamsMeetingContextSelector? selector)
    {
        ArgumentNullException.ThrowIfNull(meetingSession);

        if (selector is null)
        {
            return new TeamsContextValidationResult(true, null, null, null);
        }

        if (!string.IsNullOrWhiteSpace(selector.TeamsMeetingId)
            && !string.Equals(
                meetingSession.TeamsContextIdentifiers.TeamsMeetingId,
                selector.TeamsMeetingId.Trim(),
                StringComparison.Ordinal))
        {
            return new TeamsContextValidationResult(
                false,
                "Teams meeting context does not match the stored meeting.",
                selector.TeamsMeetingId,
                selector.TeamsChatId);
        }

        if (!string.IsNullOrWhiteSpace(selector.TeamsChatId)
            && !string.Equals(
                meetingSession.TeamsContextIdentifiers.TeamsChatId,
                selector.TeamsChatId.Trim(),
                StringComparison.Ordinal))
        {
            return new TeamsContextValidationResult(
                false,
                "Teams chat context does not match the stored meeting.",
                selector.TeamsMeetingId,
                selector.TeamsChatId);
        }

        return new TeamsContextValidationResult(true, null, selector.TeamsMeetingId, selector.TeamsChatId);
    }
}
