using DarkKitchen.Contracts.Events;
using DarkKitchen.Inventory.Features.Application;
using DarkKitchen.Inventory.Features.Features;
using DarkKitchen.Inventory.Infrastructure;
using DarkKitchen.Inventory.Infrastructure.Persistence;
using DarkKitchen.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddIntegrationEventBus("inventory-db", DarkKitchenService.Inventory, typeof(InventoryDbContext).Assembly);
builder.Services.AddCors(options =>
{
    options.AddPolicy("inventory-panel", policy =>
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
builder.Services.AddInventoryInfrastructure(builder.Configuration);
builder.Services.AddProblemDetails();

var app = builder.Build();

await app.Services.InitializeInventoryDatabaseAsync();

app.UseExceptionHandler();
app.UseCors("inventory-panel");
app.MapDefaultEndpoints();
app.MapInventoryEndpoints();

app.Run();
