using MeetingAgent.Domain.Validation;

namespace MeetingAgent.Domain.Entities;

public sealed class RecapInsight
{
    public RecapInsight(string summary, string evidenceSnippet)
    {
        Summary = DomainRules.Required(summary, nameof(summary));
        EvidenceSnippet = DomainRules.Required(evidenceSnippet, nameof(evidenceSnippet));
    }

    public string Summary { get; }

    public string EvidenceSnippet { get; }
}
