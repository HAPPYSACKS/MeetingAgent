using MeetingAgent.Application.DependencyInjection;
using MeetingAgent.Infrastructure.DependencyInjection;
using MeetingAgent.Web.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRetentionCleanupWorker();
builder.Services.AddHealthChecks();
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();
app.MapHealthChecks("/health");
app.MapMeetingStatusApi();

app.Run();

public partial class Program;
