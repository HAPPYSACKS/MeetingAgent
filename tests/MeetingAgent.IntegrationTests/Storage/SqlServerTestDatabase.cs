using MeetingAgent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeetingAgent.IntegrationTests.Storage;

internal sealed class SqlServerTestDatabase : IAsyncDisposable
{
    private readonly DbContextOptions<MeetingAgentDbContext> _options;

    public SqlServerTestDatabase()
    {
        DatabaseName = $"MeetingAgentTests_{Guid.NewGuid():N}";
        ConnectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={DatabaseName};Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True";
        _options = new DbContextOptionsBuilder<MeetingAgentDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
    }

    public string DatabaseName { get; }

    public string ConnectionString { get; }

    public MeetingAgentDbContext CreateContext() => new(_options);

    public async Task MigrateAsync()
    {
        await using var dbContext = CreateContext();
        await dbContext.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await using var dbContext = CreateContext();
        await dbContext.Database.EnsureDeletedAsync();
    }
}
