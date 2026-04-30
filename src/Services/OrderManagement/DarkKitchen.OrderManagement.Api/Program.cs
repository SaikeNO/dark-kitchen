using DarkKitchen.Contracts.Events;
using DarkKitchen.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddIntegrationEventBus("order-db", DarkKitchenService.OrderManagement);
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.MapDefaultEndpoints();

app.MapGet("/", () => Results.Ok(new
{
    service = "Order Management Service",
    boundedContext = "Orders",
    status = "ready"
}));

app.MapGet("/api/info", () => Results.Ok(new
{
    service = "Order Management Service",
    responsibilities = new[]
    {
        "Order ingestion",
        "Order state machine",
        "Saga coordination"
    }
}));

app.Run();
