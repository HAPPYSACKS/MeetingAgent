using MeetingAgent.Application.DependencyInjection;
using MeetingAgent.Application.Tools;
using MeetingAgent.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    product = "MeetingAgent.Mcp",
    description = "MCP companion host for MeetingAgent backend tools."
}));

app.MapHealthChecks("/health");
app.MapGet("/tools", () => Results.Ok(FacilitatorToolCatalog.All));

app.Run();

public partial class Program;
