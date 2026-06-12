using FluentAssertions;
using MeetingAgent.Infrastructure.DependencyInjection;
using MeetingAgent.Infrastructure.Options;
using MeetingAgent.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MeetingAgent.IntegrationTests.Storage;

public sealed class InfrastructureConfigurationTests
{
    [Fact]
    public void MeetingAgentConnectionString_UsesConfiguredConnectionStringFirst()
    {
        const string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Configured;Trusted_Connection=True";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MeetingAgent"] = connectionString,
                ["Sql:ServerName"] = "server.database.windows.net",
                ["Sql:DatabaseName"] = "MeetingAgent"
            })
            .Build();

        MeetingAgentConnectionString.Resolve(configuration, isDevelopment: false).Should().Be(connectionString);
    }

    [Fact]
    public void MeetingAgentConnectionString_BuildsAzureSqlConnectionWhenNoExplicitConnectionStringExists()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Sql:ServerName"] = "meetingagent.database.windows.net",
                ["Sql:DatabaseName"] = "MeetingAgent",
                ["Sql:UseManagedIdentity"] = "true"
            })
            .Build();

        var resolved = MeetingAgentConnectionString.Resolve(configuration, isDevelopment: false);
        var builder = new SqlConnectionStringBuilder(resolved);

        builder.DataSource.Should().Be("meetingagent.database.windows.net");
        builder.InitialCatalog.Should().Be("MeetingAgent");
        builder.Authentication.Should().Be(SqlAuthenticationMethod.ActiveDirectoryDefault);
    }

    [Fact]
    public void MeetingAgentConnectionString_FallsBackToLocalDb()
    {
        var configuration = new ConfigurationBuilder().Build();

        MeetingAgentConnectionString.Resolve(configuration, isDevelopment: true)
            .Should().Be(MeetingAgentConnectionString.DevelopmentLocalDb);
    }

    [Fact]
    public void MeetingAgentConnectionString_ThrowsWhenDatabaseConfigIsMissingOutsideDevelopment()
    {
        var configuration = new ConfigurationBuilder().Build();

        var act = () => MeetingAgentConnectionString.Resolve(configuration, isDevelopment: false);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*database configuration is missing*");
    }

    [Fact]
    public void AddInfrastructure_ValidatesRetentionCleanupOptions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MeetingAgent:RetentionCleanup:IntervalMinutes"] = "0"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        using var serviceProvider = services.BuildServiceProvider();

        var act = () => serviceProvider.GetRequiredService<IOptions<RetentionCleanupOptions>>().Value;

        act.Should().Throw<OptionsValidationException>();
    }
}
