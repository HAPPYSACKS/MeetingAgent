using Microsoft.Extensions.DependencyInjection;
using MeetingAgent.Application.Security;

namespace MeetingAgent.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMeetingAuthorizationService, MeetingAuthorizationService>();

        return services;
    }
}
