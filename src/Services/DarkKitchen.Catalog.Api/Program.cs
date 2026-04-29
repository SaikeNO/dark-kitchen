using DarkKitchen.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.MapDefaultEndpoints();

app.MapGet("/", () => Results.Ok(new
{
    service = "Catalog & Recipe Service",
    boundedContext = "Catalog",
    status = "ready"
}));

app.MapGet("/api/info", () => Results.Ok(new
{
    service = "Catalog & Recipe Service",
    responsibilities = new[]
    {
        "Brands",
        "Menu",
        "Recipes",
        "Kitchen station routing"
    }
}));

app.Run();

