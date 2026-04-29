using DarkKitchen.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.MapDefaultEndpoints();

app.MapGet("/", () => Results.Ok(new
{
    service = "Storefront Service",
    boundedContext = "Direct Sales",
    status = "ready"
}));

app.MapGet("/api/info", () => Results.Ok(new
{
    service = "Storefront Service",
    responsibilities = new[]
    {
        "White-label storefront BFF",
        "Customer identity",
        "Mock payment checkout"
    }
}));

app.Run();

