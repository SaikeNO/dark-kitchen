using DarkKitchen.Catalog.Features.Features;
using DarkKitchen.Catalog.Infrastructure;
using DarkKitchen.Catalog.Infrastructure.Persistence;
using DarkKitchen.Contracts.Events;
using DarkKitchen.ServiceDefaults;

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
builder.Services.AddCatalogInfrastructure(builder.Configuration);
builder.Services.AddProblemDetails();

var app = builder.Build();

await app.Services.InitializeCatalogDatabaseAsync();

app.UseExceptionHandler();
app.UseCors("admin-panel");
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();
app.MapCatalogEndpoints();

app.Run();
