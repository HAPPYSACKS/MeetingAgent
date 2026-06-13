using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using FluentAssertions;
using MeetingAgent.Application.Storage;
using MeetingAgent.Domain.Entities;
using MeetingAgent.Domain.Enums;
using MeetingAgent.Domain.ValueObjects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeetingAgent.IntegrationTests;

public class AuthEndpointTests(MeetingAgentWebApplicationFactory factory) : IClassFixture<MeetingAgentWebApplicationFactory>
{
    [Fact]
    public async Task GetMe_ReturnsUnauthorizedWithoutAuthentication()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_ReturnsUserForBearerIdentity()
    {
        var client = factory.WithTestAuthentication().CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", "test-token");

        var response = await client.GetAsync("/api/auth/me");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthMeResponse>();
        body.Should().NotBeNull();
        body!.TenantId.Should().Be("tenant-123");
        body.ObjectId.Should().Be("object-123");
        body.UserPrincipalName.Should().Be("host@contoso.com");
        body.AuthenticationMode.Should().Be("Bearer");
    }

    [Fact]
    public async Task GetMe_ReturnsUserForInteractiveIdentity()
    {
        var client = factory.WithTestAuthentication().CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.TestAuthHeader, "1");

        var response = await client.GetAsync("/api/auth/me");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthMeResponse>();
        body.Should().NotBeNull();
        body!.TenantId.Should().Be("tenant-123");
        body.ObjectId.Should().Be("object-123");
        body.UserPrincipalName.Should().Be("host@contoso.com");
    }

    [Fact]
    public async Task HostAccess_ReturnsForbiddenForAuthenticatedNonHost()
    {
        var meeting = new MeetingSession(
            Guid.NewGuid(),
            "host@contoso.com",
            "Planning",
            new DateTimeOffset(2026, 06, 13, 16, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2026, 06, 13, 16, 30, 00, TimeSpan.Zero),
            MeetingStatus.Scheduled,
            new TeamsMeetingContextIdentifiers("teams-meeting", "teams-chat", "calendar-event"));

        var client = factory
            .WithTestAuthentication("delegate@contoso.com")
            .WithTestMeetingSession(meeting)
            .CreateClient();

        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.TestAuthHeader, "1");
        client.DefaultRequestHeaders.Add("X-Teams-Meeting-Id", "teams-meeting");
        client.DefaultRequestHeaders.Add("X-Teams-Chat-Id", "teams-chat");

        var response = await client.GetAsync($"/api/auth/meetings/{meeting.Id}/host-access");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private sealed record AuthMeResponse(
        string TenantId,
        string ObjectId,
        string UserPrincipalName,
        string? DisplayName,
        string AuthenticationMode);
}

internal static class MeetingAgentWebApplicationFactoryAuthExtensions
{
    public static WebApplicationFactory<Program> WithTestAuthentication(
        this MeetingAgentWebApplicationFactory factory,
        string userPrincipalName = "host@contoso.com")
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(TestAuthenticationHandler.SchemeName)
                    .AddScheme<TestAuthenticationOptions, TestAuthenticationHandler>(
                        TestAuthenticationHandler.SchemeName,
                        options => options.UserPrincipalName = userPrincipalName);

                services.PostConfigure<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultScheme = TestAuthenticationHandler.SchemeName;
                });
            });
        });
    }

    public static WebApplicationFactory<Program> WithTestMeetingSession(
        this WebApplicationFactory<Program> factory,
        MeetingSession meetingSession)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IMeetingSessionRepository>(new TestMeetingSessionRepository(meetingSession));
            });
        });
    }
}

internal sealed class TestAuthenticationOptions : AuthenticationSchemeOptions
{
    public string UserPrincipalName { get; set; } = "host@contoso.com";
}

internal sealed class TestAuthenticationHandler(
    IOptionsMonitor<TestAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<TestAuthenticationOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    public const string TestAuthHeader = "X-Test-Auth";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(TestAuthHeader)
            && !Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
            new Claim("tid", "tenant-123"),
            new Claim("oid", "object-123"),
            new Claim("preferred_username", Options.UserPrincipalName),
            new Claim("name", "Test User")
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }
}

internal sealed class TestMeetingSessionRepository(MeetingSession meetingSession) : IMeetingSessionRepository
{
    public Task SaveAsync(MeetingSession meetingSession, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<MeetingSession?> GetByIdAsync(Guid meetingId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(meetingSession.Id == meetingId ? meetingSession : null);
    }
}
