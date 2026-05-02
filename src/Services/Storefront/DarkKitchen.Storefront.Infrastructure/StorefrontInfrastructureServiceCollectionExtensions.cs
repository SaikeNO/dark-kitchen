using DarkKitchen.Storefront.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Storefront.Infrastructure;

public static class StorefrontInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddStorefrontInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContextWithWolverineIntegration<StorefrontDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString("storefront-db")
                ?? throw new InvalidOperationException("Missing required connection string 'storefront-db'.");
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(StorefrontDbContextFactory).Assembly.FullName));
        });

        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies();
        services.AddAuthorization();
        services.AddIdentityCore<StorefrontUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<StorefrontDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "DarkKitchen.Storefront";
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
