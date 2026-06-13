using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace MeetingAgent.IntegrationTests;

public class UnconfiguredAuthenticationTests(MeetingAgentWebApplicationFactory factory) : IClassFixture<MeetingAgentWebApplicationFactory>
{
    [Fact]
    public async Task AnonymousTeamsConfigurePage_RemainsAvailableWhenAzureAdIsNotConfigured()
    {
        var client = factory.WithBlankAzureAdConfiguration().CreateClient();

        var response = await client.GetAsync("/Teams/Configure");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ProtectedAuthProbe_ReturnsUnauthorizedWhenAzureAdIsNotConfigured()
    {
        var client = factory.WithBlankAzureAdConfiguration().CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Headers.TryGetValues("X-MeetingAgent-Auth", out var values).Should().BeTrue();
        values.Should().Contain("NotConfigured");
    }
}

internal static class UnconfiguredAuthenticationFactoryExtensions
{
    public static WebApplicationFactory<Program> WithBlankAzureAdConfiguration(this MeetingAgentWebApplicationFactory factory)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("MeetingAgent:Authentication:Disabled", "true");
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["MeetingAgent:Authentication:Disabled"] = "true",
                    ["AzureAd:TenantId"] = "",
                    ["AzureAd:ClientId"] = "",
                    ["Teams:AllowedTenantId"] = "",
                    ["Teams:ApplicationIdUri"] = ""
                });
            });
        });
    }
}
