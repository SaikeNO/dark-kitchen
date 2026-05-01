using DarkKitchen.Catalog.Api;
using DarkKitchen.Catalog.Api.Features;
using DarkKitchen.Contracts.Events;
using DarkKitchen.ServiceDefaults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddIntegrationEventBus("catalog-db", DarkKitchenService.Catalog);
builder.Services.AddCors(options =>
{
    options.AddPolicy("admin-panel", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
            Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            && (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
                || string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(uri.Host, "::1", StringComparison.OrdinalIgnoreCase)))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddDbContextWithWolverineIntegration<CatalogDbContext>((_, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("catalog-db")
        ?? throw new InvalidOperationException("Missing required connection string 'catalog-db'.");
    options.UseNpgsql(connectionString);
});
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(CatalogPolicies.Operator, policy => policy.RequireRole(CatalogRoles.Manager, CatalogRoles.Operator));
    options.AddPolicy(CatalogPolicies.Manager, policy => policy.RequireRole(CatalogRoles.Manager));
});
builder.Services.AddIdentityCore<CatalogUser>(options =>
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
builder.Services.ConfigureApplicationCookie(options =>
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
builder.Services.AddProblemDetails();

var app = builder.Build();

await app.Services.InitializeCatalogDatabaseAsync();

app.UseExceptionHandler();
app.UseCors("admin-panel");
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();

app.MapGet("/", () => Results.Ok(new
{
    service = "Catalog & Recipe Service",
    boundedContext = "Catalog",
    status = "ready"
}));

app.MapGet("/api/info", () => Results.Ok(new
{
    service = "Catalog & Recipe Service",
    responsibilities = new[]
    {
        "Brands",
        "Menu",
        "Recipes",
        "Kitchen station routing"
    }
}));

app.MapCatalogEndpoints();

app.Run();

