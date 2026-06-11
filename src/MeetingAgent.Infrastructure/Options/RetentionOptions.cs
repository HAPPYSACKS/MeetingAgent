using System.ComponentModel.DataAnnotations;

namespace MeetingAgent.Infrastructure.Options;

public sealed class RetentionOptions
{
    public const string SectionName = "MeetingAgent:Retention";

    [Range(0, 30)]
    public int TranscriptArtifactDays { get; init; } = 3;

    [Range(1, 365)]
    public int RecapArtifactDays { get; init; } = 30;

    [Range(1, 3650)]
    public int MeetingMetadataDays { get; init; } = 180;
}
