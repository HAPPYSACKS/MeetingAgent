using MeetingAgent.Application.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;

namespace MeetingAgent.Web.Security;

public static class MeetingAgentAuthentication
{
    public const string Scheme = "MeetingAgent";

    public const string AuthenticatedPolicy = "MeetingAgent.Authenticated";

    public static IServiceCollection AddMeetingAgentAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();

        var azureAdSection = configuration.GetSection("AzureAd");
        var teamsSection = configuration.GetSection("Teams");
        var tenantId = azureAdSection["TenantId"];
        var clientId = azureAdSection["ClientId"];
        var authenticationDisabled = configuration.GetValue<bool>("MeetingAgent:Authentication:Disabled");

        if (authenticationDisabled || string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId))
        {
            services
                .AddAuthentication(Scheme)
                .AddScheme<AuthenticationSchemeOptions, UnconfiguredAuthenticationHandler>(
                    Scheme,
                    _ => { });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthenticatedPolicy, policy => policy.RequireAuthenticatedUser());
            });

            return services;
        }

        var authenticationBuilder = services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = Scheme;
                options.DefaultChallengeScheme = Scheme;
            });

        authenticationBuilder.AddPolicyScheme(Scheme, "MeetingAgent bearer token or interactive cookie", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                var authorizationHeader = context.Request.Headers.Authorization.ToString();
                return authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? JwtBearerDefaults.AuthenticationScheme
                    : CookieAuthenticationDefaults.AuthenticationScheme;
            };
        });

        authenticationBuilder.AddMicrosoftIdentityWebApp(
            azureAdSection,
            OpenIdConnectDefaults.AuthenticationScheme,
            CookieAuthenticationDefaults.AuthenticationScheme);

        authenticationBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var applicationIdUri = teamsSection["ApplicationIdUri"];
                var validAudiences = new[] { applicationIdUri, clientId }
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                options.Authority = $"https://login.microsoftonline.com/{ResolveTenantSegment(tenantId)}/v2.0";
                options.Audience = validAudiences.FirstOrDefault();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = validAudiences.Length > 0,
                    ValidAudiences = validAudiences,
                    ValidateIssuer = !string.IsNullOrWhiteSpace(tenantId)
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var allowedTenantId = teamsSection["AllowedTenantId"];
                        if (string.IsNullOrWhiteSpace(allowedTenantId))
                        {
                            allowedTenantId = tenantId;
                        }

                        if (!string.IsNullOrWhiteSpace(allowedTenantId))
                        {
                            var tokenTenantId = context.Principal?.FindFirst("tid")?.Value
                                ?? context.Principal?.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

                            if (!string.Equals(tokenTenantId, allowedTenantId, StringComparison.OrdinalIgnoreCase))
                            {
                                context.Fail("Token tenant is not allowed for this MeetingAgent host.");
                            }
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthenticatedPolicy, policy => policy.RequireAuthenticatedUser());
        });

        return services;
    }

    private static string ResolveTenantSegment(string? tenantId)
    {
        return string.IsNullOrWhiteSpace(tenantId) ? "common" : tenantId.Trim();
    }
}
