using System.ComponentModel.DataAnnotations;

namespace MeetingAgent.Infrastructure.Options;

public sealed class RetentionCleanupOptions
{
    public const string SectionName = "MeetingAgent:RetentionCleanup";

    public bool Enabled { get; init; } = true;

    [Range(1, 10080)]
    public int IntervalMinutes { get; init; } = 1440;
}
