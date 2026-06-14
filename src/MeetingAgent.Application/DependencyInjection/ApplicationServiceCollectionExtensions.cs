using Microsoft.Extensions.DependencyInjection;
using MeetingAgent.Application.Alerts;
using MeetingAgent.Application.Agenda;
using MeetingAgent.Application.Pacing;
using MeetingAgent.Application.Security;

namespace MeetingAgent.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAgendaDraftingService, HeuristicAgendaDraftingService>();
        services.AddScoped<IPacingAlertService, PacingAlertService>();
        services.AddScoped<IPacingEngine, PacingEngine>();
        services.AddScoped<IMeetingAuthorizationService, MeetingAuthorizationService>();

        return services;
    }
}
