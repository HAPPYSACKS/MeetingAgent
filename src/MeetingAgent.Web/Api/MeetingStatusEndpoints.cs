using MeetingAgent.Application.Tools;

namespace MeetingAgent.Web.Api;

public static class MeetingStatusEndpoints
{
    public static IEndpointRouteBuilder MapMeetingStatusApi(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api").WithTags("Meeting");

        group.MapGet("/status", () =>
        {
            return Results.Ok(new
            {
                product = "MeetingAgent",
                phase = "Phase 1 internal pilot",
                mode = "host-private, transcript-first",
                surfaces = new[]
                {
                    "Teams meeting side panel",
                    "Host setup experience",
                    "Post-meeting recap view"
                },
                tools = FacilitatorToolCatalog.All.Select(tool => tool.Name)
            });
        });

        return endpoints;
    }
}
