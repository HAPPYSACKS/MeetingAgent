using FluentAssertions;

namespace MeetingAgent.IntegrationTests;

public class StatusEndpointTests(MeetingAgentWebApplicationFactory factory) : IClassFixture<MeetingAgentWebApplicationFactory>
{
    [Fact]
    public async Task GetStatus_ReturnsSuccessAndMeetingAgentPayload()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/status");

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();

        body.Should().Contain("MeetingAgent");
        body.Should().Contain("create_agenda_draft");
    }

    [Fact]
    public async Task GetHealth_ReturnsSuccessWithoutAuthentication()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }
}
