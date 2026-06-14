using FluentAssertions;
using MeetingAgent.Application.Agenda;

namespace MeetingAgent.UnitTests;

public sealed class AgendaDraftingServiceTests
{
    private readonly HeuristicAgendaDraftingService _service = new();

    [Theory]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(90)]
    public void CreateDraft_FitsSectionsInsideRequestedDuration(int durationMinutes)
    {
        var draft = _service.CreateDraft(new AgendaDraftRequest(
            "Decide the launch checklist for the pilot",
            TimeSpan.FromMinutes(durationMinutes),
            "Keep risk discussion concise."));

        draft.TotalDuration.Should().Be(TimeSpan.FromMinutes(durationMinutes));
        draft.Sections.Should().NotBeEmpty();
        draft.Sections.Sum(section => section.SuggestedDuration.TotalMinutes)
            .Should().BeLessThanOrEqualTo(durationMinutes);
    }

    [Fact]
    public void CreateDraft_IncludesFacilitationNotes()
    {
        var draft = _service.CreateDraft(new AgendaDraftRequest(
            "Prioritize the next implementation slice",
            TimeSpan.FromMinutes(45),
            "Sam needs space to explain deployment risk."));

        draft.Sections.Should().Contain(section => !string.IsNullOrWhiteSpace(section.FacilitationNotes));
    }

    [Theory]
    [InlineData("Talk about the project")]
    [InlineData("Review product scope, implementation sequencing, pilot rollout, stakeholder alignment, deployment risk, privacy expectations, transcript handling, and all remaining roadmap questions")]
    public void CreateDraft_HandlesVagueAndBroadGoals(string goal)
    {
        var draft = _service.CreateDraft(new AgendaDraftRequest(goal, TimeSpan.FromMinutes(45)));

        draft.Sections.Should().HaveCountGreaterThan(1);
        draft.Sections.Sum(section => section.SuggestedDuration.TotalMinutes)
            .Should().BeLessThanOrEqualTo(45);
    }

    [Fact]
    public void CreateDraft_RejectsMissingGoal()
    {
        var act = () => _service.CreateDraft(new AgendaDraftRequest("", TimeSpan.FromMinutes(30)));

        act.Should().Throw<ArgumentException>();
    }
}
