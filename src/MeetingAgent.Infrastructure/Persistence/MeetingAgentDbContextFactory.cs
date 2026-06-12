using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MeetingAgent.Infrastructure.Persistence;

public sealed class MeetingAgentDbContextFactory : IDesignTimeDbContextFactory<MeetingAgentDbContext>
{
    public MeetingAgentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MeetingAgentDbContext>();
        optionsBuilder.UseSqlServer(MeetingAgentConnectionString.DevelopmentLocalDb);
        return new MeetingAgentDbContext(optionsBuilder.Options);
    }
}
