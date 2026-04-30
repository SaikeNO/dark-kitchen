using DarkKitchen.Contracts.Events;
using DarkKitchen.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddIntegrationEventBus("packing-db", DarkKitchenService.Packing);
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.MapDefaultEndpoints();

app.MapGet("/", () => Results.Ok(new
{
    service = "Packing Service",
    boundedContext = "Packing",
    status = "ready"
}));

app.MapGet("/api/info", () => Results.Ok(new
{
    service = "Packing Service",
    responsibilities = new[]
    {
        "Packing manifest",
        "Event aggregation",
        "Courier handoff"
    }
}));

app.Run();
