using DarkKitchen.Contracts.Events;
using DarkKitchen.ServiceDefaults;
using DarkKitchen.Storefront.Features.Application;
using DarkKitchen.Storefront.Features.Features;
using DarkKitchen.Storefront.Infrastructure;
using DarkKitchen.Storefront.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddIntegrationEventBus("storefront-db", DarkKitchenService.Storefront, typeof(StorefrontDbContext).Assembly);
builder.Services.AddCors(options =>
{
    options.AddPolicy("storefront", policy =>
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
builder.Services.AddStorefrontInfrastructure(builder.Configuration);
builder.Services.AddHttpClient<OrderManagementClient>(client =>
{
    var baseUrl = builder.Configuration["OrderManagement:BaseUrl"] ?? "http://order-management-api";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddProblemDetails();

var app = builder.Build();

await app.Services.InitializeStorefrontDatabaseAsync();

app.UseExceptionHandler();
app.UseCors("storefront");
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();
app.MapStorefrontEndpoints();

app.Run();
