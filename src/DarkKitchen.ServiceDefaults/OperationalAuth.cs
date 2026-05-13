using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DarkKitchen.ServiceDefaults;

public static class OperationalAuth
{
    public const string Scheme = "DarkKitchenOperational";
    public const string RoleHeader = "X-DarkKitchen-Role";
    public const string ManagerRole = "Manager";
    public const string OperatorRole = "Operator";
    public const string ManagerPolicy = "ops.manager";
    public const string OperatorPolicy = "ops.operator";

    public static IServiceCollection AddOperationalAuth(this IServiceCollection services)
    {
        services.AddAuthentication(Scheme)
            .AddScheme<AuthenticationSchemeOptions, OperationalAuthHandler>(Scheme, _ => { });
        services.AddAuthorization(options =>
        {
            options.AddPolicy(OperatorPolicy, policy => policy.RequireRole(ManagerRole, OperatorRole));
            options.AddPolicy(ManagerPolicy, policy => policy.RequireRole(ManagerRole));
        });

        return services;
    }
}

internal sealed class OperationalAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var role = Request.Headers[OperationalAuth.RoleHeader].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(role))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        role = role.Trim();
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "operational-user"),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, OperationalAuth.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, OperationalAuth.Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
