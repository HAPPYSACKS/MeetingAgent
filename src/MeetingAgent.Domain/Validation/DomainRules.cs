namespace MeetingAgent.Domain.Validation;

internal static class DomainRules
{
    public static readonly TimeSpan MinMeetingDuration = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan MaxMeetingDuration = TimeSpan.FromHours(8);
    public static readonly TimeSpan MinAgendaSectionDuration = TimeSpan.FromMinutes(1);

    public static string Required(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }

    public static void EnsureMeetingDuration(TimeSpan duration, string parameterName)
    {
        if (duration < MinMeetingDuration || duration > MaxMeetingDuration)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                duration,
                $"Meeting duration must be between {MinMeetingDuration} and {MaxMeetingDuration}.");
        }
    }

    public static void EnsureAgendaSectionDuration(TimeSpan duration, string parameterName)
    {
        if (duration < MinAgendaSectionDuration)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                duration,
                $"Agenda section duration must be at least {MinAgendaSectionDuration}.");
        }
    }

    public static void EnsureEnumValue<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, $"Invalid {typeof(TEnum).Name} value.");
        }
    }
}
