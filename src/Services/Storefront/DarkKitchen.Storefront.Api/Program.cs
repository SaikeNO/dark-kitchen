using DarkKitchen.Contracts.Events;
using DarkKitchen.ServiceDefaults;
using DarkKitchen.Storefront.Features.Features.ServiceInfo;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddIntegrationEventBus("storefront-db", DarkKitchenService.Storefront);
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.MapDefaultEndpoints();
app.MapServiceInfoEndpoints();

app.Run();
