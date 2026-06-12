using MeetingAgent.Infrastructure.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace MeetingAgent.Infrastructure.Persistence;

public static class MeetingAgentConnectionString
{
    public const string Name = "MeetingAgent";
    public const string DevelopmentLocalDb =
        "Server=(localdb)\\MSSQLLocalDB;Database=MeetingAgent;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True";

    public static string Resolve(IConfiguration configuration)
    {
        var configuredConnectionString = configuration.GetConnectionString(Name);
        if (!string.IsNullOrWhiteSpace(configuredConnectionString))
        {
            return configuredConnectionString;
        }

        var sqlOptions = configuration.GetSection(SqlOptions.SectionName).Get<SqlOptions>() ?? new SqlOptions();
        if (!string.IsNullOrWhiteSpace(sqlOptions.ServerName) && !string.IsNullOrWhiteSpace(sqlOptions.DatabaseName))
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = sqlOptions.ServerName,
                InitialCatalog = sqlOptions.DatabaseName,
                Encrypt = true,
                TrustServerCertificate = false,
                MultipleActiveResultSets = true
            };

            if (sqlOptions.UseManagedIdentity)
            {
                builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryDefault;
            }

            return builder.ConnectionString;
        }

        return DevelopmentLocalDb;
    }
}
