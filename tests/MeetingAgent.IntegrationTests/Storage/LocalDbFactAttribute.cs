namespace MeetingAgent.IntegrationTests.Storage;

public sealed class LocalDbFactAttribute : FactAttribute
{
    public LocalDbFactAttribute()
    {
        if (!SqlServerTestDatabase.IsLocalDbAvailable())
        {
            Skip = "SQL Server LocalDB is not installed or available on this machine.";
        }
    }
}
