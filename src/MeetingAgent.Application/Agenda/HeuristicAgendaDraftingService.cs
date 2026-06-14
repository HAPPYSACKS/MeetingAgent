namespace MeetingAgent.Application.Agenda;

public sealed class HeuristicAgendaDraftingService : IAgendaDraftingService
{
    public AgendaDraft CreateDraft(AgendaDraftRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var goal = Require(request.MeetingGoal, nameof(request.MeetingGoal));
        EnsureMeetingDuration(request.Duration, nameof(request.Duration));

        var totalMinutes = (int)Math.Round(request.Duration.TotalMinutes, MidpointRounding.AwayFromZero);
        var notesHint = string.IsNullOrWhiteSpace(request.HostNotes)
            ? "Keep the host focused on the stated outcome and watch for drift."
            : $"Use host notes as private context: {request.HostNotes.Trim()}";

        var sections = totalMinutes switch
        {
            <= 20 => BuildShortAgenda(totalMinutes, goal, notesHint),
            <= 60 => BuildStandardAgenda(totalMinutes, goal, notesHint),
            _ => BuildExtendedAgenda(totalMinutes, goal, notesHint)
        };

        return new AgendaDraft(goal, request.Duration, sections);
    }

    private static string Require(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }

    private static void EnsureMeetingDuration(TimeSpan duration, string parameterName)
    {
        if (duration < TimeSpan.FromMinutes(5) || duration > TimeSpan.FromHours(8))
        {
            throw new ArgumentOutOfRangeException(parameterName, duration, "Meeting duration must be between 5 minutes and 8 hours.");
        }
    }

    private static IReadOnlyList<AgendaDraftSection> BuildShortAgenda(int totalMinutes, string goal, string notesHint)
    {
        var opening = Math.Max(3, totalMinutes / 5);
        var close = Math.Max(3, totalMinutes / 5);
        var working = totalMinutes - opening - close;

        return
        [
            new("Frame outcome", $"Confirm the desired outcome: {goal}", TimeSpan.FromMinutes(opening), 0, "Ask the host to name the decision or deliverable in one sentence."),
            new("Work the core topic", "Use the remaining time for the highest-value discussion.", TimeSpan.FromMinutes(working), 1, notesHint),
            new("Confirm next step", "Summarize the decision, owner, or follow-up before the meeting ends.", TimeSpan.FromMinutes(close), 2, "Prompt for one clear owner or next action.")
        ];
    }

    private static IReadOnlyList<AgendaDraftSection> BuildStandardAgenda(int totalMinutes, string goal, string notesHint)
    {
        var opening = Math.Max(5, (int)Math.Round(totalMinutes * 0.15));
        var context = Math.Max(5, (int)Math.Round(totalMinutes * 0.2));
        var decision = Math.Max(10, (int)Math.Round(totalMinutes * 0.45));
        var close = totalMinutes - opening - context - decision;

        if (close < 5)
        {
            var needed = 5 - close;
            decision -= needed;
            close = 5;
        }

        return
        [
            new("Frame goal and constraints", $"Align the group around the meeting outcome: {goal}", TimeSpan.FromMinutes(opening), 0, "Keep framing tight; avoid re-litigating why the meeting exists."),
            new("Share essential context", "Surface only the context needed to make progress.", TimeSpan.FromMinutes(context), 1, notesHint),
            new("Discuss options and decide", "Compare the viable paths and move toward the host's needed outcome.", TimeSpan.FromMinutes(decision), 2, "Watch for circular debate and suggest parking lower-priority threads."),
            new("Close with commitments", "Confirm decisions, owners, and the first follow-up action.", TimeSpan.FromMinutes(close), 3, "Leave enough time for ownership, not just summary.")
        ];
    }

    private static IReadOnlyList<AgendaDraftSection> BuildExtendedAgenda(int totalMinutes, string goal, string notesHint)
    {
        var opening = Math.Max(8, (int)Math.Round(totalMinutes * 0.1));
        var context = Math.Max(10, (int)Math.Round(totalMinutes * 0.15));
        var exploration = Math.Max(20, (int)Math.Round(totalMinutes * 0.3));
        var decision = Math.Max(20, (int)Math.Round(totalMinutes * 0.3));
        var close = totalMinutes - opening - context - exploration - decision;

        if (close < 10)
        {
            var needed = 10 - close;
            exploration -= needed / 2;
            decision -= needed - needed / 2;
            close = 10;
        }

        return
        [
            new("Frame goal and success criteria", $"Define what success means for: {goal}", TimeSpan.FromMinutes(opening), 0, "Ask for explicit constraints and decision rules up front."),
            new("Establish shared context", "Review the minimum background everyone needs.", TimeSpan.FromMinutes(context), 1, notesHint),
            new("Explore options", "Compare options, risks, and tradeoffs without forcing an early conclusion.", TimeSpan.FromMinutes(exploration), 2, "If discussion branches, name the branch and park it visibly."),
            new("Converge on decision", "Move from discussion to decision, recommendation, or narrowed next step.", TimeSpan.FromMinutes(decision), 3, "Prompt the host to test for objections before closing."),
            new("Commit and close", "Confirm owners, due dates, and what will be communicated after the meeting.", TimeSpan.FromMinutes(close), 4, "Protect this time even if the middle runs long.")
        ];
    }
}
