namespace MeetingAgent.Application.Tools;

public sealed record FacilitatorToolDefinition(string Name, string Description);

public static class FacilitatorToolCatalog
{
    public static IReadOnlyList<FacilitatorToolDefinition> All { get; } =
    [
        new("create_agenda_draft", "Create a draft agenda from the meeting objective, duration, and host notes."),
        new("explain_pacing_risk", "Explain why the current pacing is at risk and what schedule pressure is building."),
        new("suggest_host_intervention", "Suggest a short, private intervention the host can use to recover the meeting."),
        new("generate_host_recap", "Generate a concise post-meeting recap for the host."),
        new("get_meeting_status", "Return the current host-facing meeting status, pacing, and alert context.")
    ];
}
