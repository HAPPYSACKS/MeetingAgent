using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace MeetingAgent.IntegrationTests;

public sealed class MeetingAgentWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureAd:Instance"] = "https://login.microsoftonline.com/",
                ["AzureAd:TenantId"] = "tenant-123",
                ["AzureAd:ClientId"] = "client-123",
                ["AzureAd:Domain"] = "contoso.com",
                ["AzureAd:CallbackPath"] = "/signin-oidc",
                ["AzureAd:SignedOutCallbackPath"] = "/signout-callback-oidc",
                ["Teams:ApplicationIdUri"] = "api://localhost/client-123",
                ["Teams:AllowedTenantId"] = "tenant-123"
            });
        });
    }
}
