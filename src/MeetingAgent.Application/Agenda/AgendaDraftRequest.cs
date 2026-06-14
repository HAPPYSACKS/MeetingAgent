namespace MeetingAgent.Application.Agenda;

public sealed record AgendaDraftRequest(
    string MeetingGoal,
    TimeSpan Duration,
    string? HostNotes = null,
    string? MeetingTitle = null);
