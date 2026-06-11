using FluentAssertions;
using MeetingAgent.Application.Tools;

namespace MeetingAgent.UnitTests;

public class FacilitatorToolCatalogTests
{
    [Fact]
    public void All_ContainsExpectedPhaseOneTools()
    {
        var toolNames = FacilitatorToolCatalog.All.Select(tool => tool.Name).ToArray();

        toolNames.Should().Contain([
            "create_agenda_draft",
            "explain_pacing_risk",
            "suggest_host_intervention",
            "generate_host_recap",
            "get_meeting_status"
        ]);
    }
}
