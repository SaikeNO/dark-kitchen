using DarkKitchen.Contracts.Events;
using DarkKitchen.ServiceDefaults;
using DarkKitchen.Packing.Features.Application;
using DarkKitchen.Packing.Features.Features.Packing;
using DarkKitchen.Packing.Features.Features.ServiceInfo;
using DarkKitchen.Packing.Infrastructure;
using DarkKitchen.Packing.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddIntegrationEventBus("packing-db", DarkKitchenService.Packing, typeof(PackingDbContext).Assembly);
builder.Services.AddCors(options =>
{
    options.AddPolicy("packing-terminal", policy =>
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
builder.Services.AddPackingInfrastructure(builder.Configuration);
builder.Services.AddOperationalAuth();
builder.Services.AddSignalR();
builder.Services.AddProblemDetails();

var app = builder.Build();

await app.Services.InitializePackingDatabaseAsync();

app.UseExceptionHandler();
app.UseCors("packing-terminal");
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();
app.MapServiceInfoEndpoints();
app.MapPackingEndpoints();
app.MapHub<PackingHub>("/hubs/packing");

app.Run();
