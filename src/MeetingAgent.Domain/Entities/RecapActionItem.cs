using MeetingAgent.Domain.Validation;

namespace MeetingAgent.Domain.Entities;

public sealed class RecapActionItem
{
    public RecapActionItem(string description, string? owner = null)
    {
        Description = DomainRules.Required(description, nameof(description));
        Owner = string.IsNullOrWhiteSpace(owner) ? null : owner.Trim();
    }

    public string Description { get; }

    public string? Owner { get; }
}
