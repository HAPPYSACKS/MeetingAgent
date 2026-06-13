using System.Security.Claims;
using FluentAssertions;
using MeetingAgent.Application.Security;
using MeetingAgent.Application.Storage;
using MeetingAgent.Domain.Entities;
using MeetingAgent.Domain.Enums;
using MeetingAgent.Domain.ValueObjects;
using MeetingAgent.Infrastructure.Security;
using Microsoft.Extensions.Logging;

namespace MeetingAgent.UnitTests;

public class SecurityTests
{
    [Fact]
    public void CurrentUserClaimsReader_PrefersStableEntraClaims()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("tid", "tenant-123"),
            new Claim("oid", "object-123"),
            new Claim("preferred_username", "host@contoso.com"),
            new Claim("upn", "fallback@contoso.com"),
            new Claim("name", "Host User")
        ], "Test"));

        var success = CurrentUserClaimsReader.TryRead(principal, "Bearer", out var user);

        success.Should().BeTrue();
        user.TenantId.Should().Be("tenant-123");
        user.ObjectId.Should().Be("object-123");
        user.UserPrincipalName.Should().Be("host@contoso.com");
        user.DisplayName.Should().Be("Host User");
        user.AuthenticationMode.Should().Be("Bearer");
    }

    [Fact]
    public void CurrentUserClaimsReader_RejectsMissingTenantOrUserIdentity()
    {
        var missingTenant = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("oid", "object-123"),
            new Claim("preferred_username", "host@contoso.com")
        ], "Test"));

        var missingObjectId = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("tid", "tenant-123"),
            new Claim("preferred_username", "host@contoso.com")
        ], "Test"));

        CurrentUserClaimsReader.TryRead(missingTenant, "Bearer", out _).Should().BeFalse();
        CurrentUserClaimsReader.TryRead(missingObjectId, "Bearer", out _).Should().BeFalse();
    }

    [Fact]
    public async Task MeetingAuthorizationService_AllowsOrganizerCaseInsensitively()
    {
        var meeting = CreateMeeting("host@contoso.com");
        var service = new MeetingAuthorizationService(
            new StubCurrentUserContext(new AuthenticatedUser("tenant", "object", "HOST@contoso.com", "Host", "Test")),
            new StubMeetingSessionRepository(meeting),
            new StubAuditLogger());

        var result = await service.AuthorizeHostAccessAsync(meeting.Id);

        result.IsAuthorized.Should().BeTrue();
        result.MeetingSession.Should().BeSameAs(meeting);
    }

    [Fact]
    public async Task MeetingAuthorizationService_DeniesNonOrganizer()
    {
        var meeting = CreateMeeting("host@contoso.com");
        var auditLogger = new StubAuditLogger();
        var service = new MeetingAuthorizationService(
            new StubCurrentUserContext(new AuthenticatedUser("tenant", "object", "delegate@contoso.com", "Delegate", "Test")),
            new StubMeetingSessionRepository(meeting),
            auditLogger);

        var result = await service.AuthorizeHostAccessAsync(meeting.Id);

        result.IsAuthorized.Should().BeFalse();
        auditLogger.UnauthorizedAccessReasons.Should().Contain("Authenticated user is not the meeting organizer.");
    }

    [Fact]
    public void StructuredAuditLogger_EmitsExpectedEventIdsWithoutSensitiveContent()
    {
        var logger = new CapturingLogger<StructuredAuditLogger>();
        var auditLogger = new StructuredAuditLogger(logger);
        var actor = new AuthenticatedUser("tenant", "object", "host@contoso.com", "Host", "Test");
        var meetingId = Guid.NewGuid();

        auditLogger.LogAgendaApproved(meetingId, actor);
        auditLogger.LogRecapAccessed(meetingId, actor);
        auditLogger.LogUnauthorizedMeetingAccess(meetingId, actor, "Denied without transcript text.");
        auditLogger.LogTeamsContextValidation(
            meetingId,
            actor,
            new TeamsContextValidationResult(true, null, "teams-meeting", "chat"));

        logger.Entries.Select(entry => entry.EventId.Id).Should().Contain([
            AuditEventIds.AgendaApproved,
            AuditEventIds.RecapAccessed,
            AuditEventIds.UnauthorizedMeetingAccess,
            AuditEventIds.TeamsContextValidation
        ]);
        logger.Entries.Should().OnlyContain(entry => !entry.Message.Contains("raw transcript", StringComparison.OrdinalIgnoreCase));
    }

    private static MeetingSession CreateMeeting(string organizerIdentity)
    {
        return new MeetingSession(
            Guid.NewGuid(),
            organizerIdentity,
            "Planning",
            new DateTimeOffset(2026, 06, 13, 16, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2026, 06, 13, 16, 30, 00, TimeSpan.Zero),
            MeetingStatus.Scheduled,
            new TeamsMeetingContextIdentifiers("teams-meeting", "teams-chat", "calendar-event"));
    }

    private sealed class StubCurrentUserContext(AuthenticatedUser? user) : ICurrentUserContext
    {
        public bool TryGetCurrentUser(out AuthenticatedUser currentUser)
        {
            currentUser = user!;
            return user is not null;
        }

        public AuthenticatedUser GetRequiredCurrentUser()
        {
            return user ?? throw new InvalidOperationException();
        }
    }

    private sealed class StubMeetingSessionRepository(MeetingSession? meetingSession) : IMeetingSessionRepository
    {
        public Task SaveAsync(MeetingSession meetingSession, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<MeetingSession?> GetByIdAsync(Guid meetingId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(meetingSession?.Id == meetingId ? meetingSession : null);
        }
    }

    private sealed class StubAuditLogger : IAuditLogger
    {
        public List<string> UnauthorizedAccessReasons { get; } = [];

        public void LogAgendaApproved(Guid meetingId, AuthenticatedUser actor)
        {
        }

        public void LogRecapAccessed(Guid meetingId, AuthenticatedUser actor)
        {
        }

        public void LogUnauthorizedMeetingAccess(Guid meetingId, AuthenticatedUser? actor, string reason)
        {
            UnauthorizedAccessReasons.Add(reason);
        }

        public void LogTeamsContextValidation(Guid meetingId, AuthenticatedUser actor, TeamsContextValidationResult result)
        {
        }
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<CapturedLogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new CapturedLogEntry(logLevel, eventId, formatter(state, exception)));
        }
    }

    private sealed record CapturedLogEntry(LogLevel LogLevel, EventId EventId, string Message);
}
