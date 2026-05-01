using DarkKitchen.Contracts.Events;
using DarkKitchen.ServiceDefaults;
using DarkKitchen.Inventory.Features.Features.ServiceInfo;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddIntegrationEventBus("inventory-db", DarkKitchenService.Inventory);
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.MapDefaultEndpoints();
app.MapServiceInfoEndpoints();

app.Run();
