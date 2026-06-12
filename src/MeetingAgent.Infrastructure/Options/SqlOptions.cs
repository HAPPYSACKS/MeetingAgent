using System.ComponentModel.DataAnnotations;

namespace MeetingAgent.Infrastructure.Options;

public sealed class SqlOptions
{
    public const string SectionName = "Sql";

    public string? ServerName { get; init; }

    public string? DatabaseName { get; init; }

    public bool UseManagedIdentity { get; init; } = true;

    [Range(1, 300)]
    public int CommandTimeoutSeconds { get; init; } = 30;
}
