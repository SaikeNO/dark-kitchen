using DarkKitchen.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.MapDefaultEndpoints();

app.MapGet("/", () => Results.Ok(new
{
    service = "KDS Service",
    boundedContext = "Kitchen",
    status = "ready"
}));

app.MapGet("/api/info", () => Results.Ok(new
{
    service = "KDS Service",
    responsibilities = new[]
    {
        "Kitchen tickets",
        "Station task routing",
        "Realtime kitchen display"
    }
}));

app.Run();

