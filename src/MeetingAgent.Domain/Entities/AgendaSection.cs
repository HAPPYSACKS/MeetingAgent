using MeetingAgent.Domain.Validation;

namespace MeetingAgent.Domain.Entities;

public sealed class AgendaSection
{
    public AgendaSection(string title, string purpose, TimeSpan suggestedDuration, int order, string? facilitationNotes = null)
    {
        Title = DomainRules.Required(title, nameof(title));
        Purpose = DomainRules.Required(purpose, nameof(purpose));
        DomainRules.EnsureAgendaSectionDuration(suggestedDuration, nameof(suggestedDuration));

        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(order), order, "Agenda section order cannot be negative.");
        }

        SuggestedDuration = suggestedDuration;
        Order = order;
        FacilitationNotes = string.IsNullOrWhiteSpace(facilitationNotes) ? null : facilitationNotes.Trim();
    }

    public string Title { get; }

    public string Purpose { get; }

    public TimeSpan SuggestedDuration { get; }

    public int Order { get; }

    public string? FacilitationNotes { get; }
}
