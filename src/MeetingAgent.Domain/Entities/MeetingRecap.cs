using MeetingAgent.Domain.Enums;
using MeetingAgent.Domain.Validation;

namespace MeetingAgent.Domain.Entities;

public sealed class MeetingRecap
{
    private readonly IReadOnlyList<RecapInsight> _topicDriftMoments;
    private readonly IReadOnlyList<RecapInsight> _confusionMoments;
    private readonly IReadOnlyList<RecapActionItem> _actionItems;

    public MeetingRecap(
        Guid id,
        Guid meetingId,
        string pacingSummary,
        IEnumerable<RecapInsight> topicDriftMoments,
        IEnumerable<RecapInsight> confusionMoments,
        IEnumerable<RecapActionItem> actionItems,
        DateTimeOffset generatedAt,
        TranscriptAvailability transcriptAvailability,
        RecapStatus status)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Recap id is required.", nameof(id));
        }

        if (meetingId == Guid.Empty)
        {
            throw new ArgumentException("Meeting id is required.", nameof(meetingId));
        }

        DomainRules.EnsureEnumValue(transcriptAvailability, nameof(transcriptAvailability));
        DomainRules.EnsureEnumValue(status, nameof(status));

        Id = id;
        MeetingId = meetingId;
        PacingSummary = DomainRules.Required(pacingSummary, nameof(pacingSummary));
        _topicDriftMoments = Array.AsReadOnly((topicDriftMoments ?? throw new ArgumentNullException(nameof(topicDriftMoments))).ToArray());
        _confusionMoments = Array.AsReadOnly((confusionMoments ?? throw new ArgumentNullException(nameof(confusionMoments))).ToArray());
        _actionItems = Array.AsReadOnly((actionItems ?? throw new ArgumentNullException(nameof(actionItems))).ToArray());
        GeneratedAt = generatedAt;
        TranscriptAvailability = transcriptAvailability;
        Status = status;
    }

    public Guid Id { get; }

    public Guid MeetingId { get; }

    public string PacingSummary { get; }

    public IReadOnlyList<RecapInsight> TopicDriftMoments => _topicDriftMoments;

    public IReadOnlyList<RecapInsight> ConfusionMoments => _confusionMoments;

    public IReadOnlyList<RecapActionItem> ActionItems => _actionItems;

    public DateTimeOffset GeneratedAt { get; }

    public TranscriptAvailability TranscriptAvailability { get; }

    public RecapStatus Status { get; private set; }

    public void MarkStatus(RecapStatus status)
    {
        DomainRules.EnsureEnumValue(status, nameof(status));
        Status = status;
    }
}
