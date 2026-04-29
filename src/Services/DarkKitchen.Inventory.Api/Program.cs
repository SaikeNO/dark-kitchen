using DarkKitchen.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.MapDefaultEndpoints();

app.MapGet("/", () => Results.Ok(new
{
    service = "Inventory Service",
    boundedContext = "Inventory",
    status = "ready"
}));

app.MapGet("/api/info", () => Results.Ok(new
{
    service = "Inventory Service",
    responsibilities = new[]
    {
        "Stock tracking",
        "Recipe read model",
        "Stock reservation"
    }
}));

app.Run();

