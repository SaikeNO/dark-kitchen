using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine.EntityFrameworkCore;
using DarkKitchen.Catalog.Infrastructure.Persistence;

namespace DarkKitchen.Catalog.Infrastructure;

public static class CatalogInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContextWithWolverineIntegration<CatalogDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString("catalog-db")
                ?? throw new InvalidOperationException("Missing required connection string 'catalog-db'.");
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(CatalogDbContextFactory).Assembly.FullName));
        });

        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies();
        services.AddAuthorization(options =>
        {
            options.AddPolicy(CatalogPolicies.Operator, policy => policy.RequireRole(CatalogRoles.Manager, CatalogRoles.Operator));
            options.AddPolicy(CatalogPolicies.Manager, policy => policy.RequireRole(CatalogRoles.Manager));
        });
        services.AddIdentityCore<CatalogUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<CatalogDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "DarkKitchen.Admin";
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        return services;
    }
}
