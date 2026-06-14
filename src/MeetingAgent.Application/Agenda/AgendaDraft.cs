namespace MeetingAgent.Application.Agenda;

public sealed record AgendaDraft(
    string Objective,
    TimeSpan TotalDuration,
    IReadOnlyList<AgendaDraftSection> Sections);

public sealed record AgendaDraftSection(
    string Title,
    string Purpose,
    TimeSpan SuggestedDuration,
    int Order,
    string? FacilitationNotes);
