using MeetingAgent.Application.DependencyInjection;
using MeetingAgent.Infrastructure.DependencyInjection;
using MeetingAgent.Web.Api;
using MeetingAgent.Web.Security;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRetentionCleanupWorker();
builder.Services.AddMeetingAgentAuthentication(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddAntiforgery(options => options.SuppressXFrameOptionsHeader = true);
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapMeetingStatusApi();
app.MapAuthApi();

app.Run();

public partial class Program;
