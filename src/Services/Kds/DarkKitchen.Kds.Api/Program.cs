using DarkKitchen.Contracts.Events;
using DarkKitchen.ServiceDefaults;
using DarkKitchen.Kds.Features.Application;
using DarkKitchen.Kds.Features.Features.ServiceInfo;
using DarkKitchen.Kds.Features.Features.Kitchen;
using DarkKitchen.Kds.Infrastructure;
using DarkKitchen.Kds.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddIntegrationEventBus("kds-db", DarkKitchenService.Kds, typeof(KdsDbContext).Assembly);
builder.Services.AddCors(options =>
{
    options.AddPolicy("kitchen-app", policy =>
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
builder.Services.AddKdsInfrastructure(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddProblemDetails();

var app = builder.Build();

await app.Services.InitializeKdsDatabaseAsync();

app.UseExceptionHandler();
app.UseCors("kitchen-app");
app.MapDefaultEndpoints();
app.MapServiceInfoEndpoints();
app.MapKitchenEndpoints();
app.MapHub<KitchenHub>("/hubs/kitchen");

app.Run();
