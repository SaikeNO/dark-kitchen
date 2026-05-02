using DarkKitchen.Contracts.Events;
using DarkKitchen.OrderManagement.Features.Application;
using DarkKitchen.OrderManagement.Features.Features;
using DarkKitchen.OrderManagement.Infrastructure;
using DarkKitchen.OrderManagement.Infrastructure.Persistence;
using DarkKitchen.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddIntegrationEventBus("order-db", DarkKitchenService.OrderManagement, typeof(OrderManagementDbContext).Assembly);
builder.Services.AddOrderManagementInfrastructure(builder.Configuration);
builder.Services.AddProblemDetails();

var app = builder.Build();

await app.Services.InitializeOrderManagementDatabaseAsync();

app.UseExceptionHandler();
app.MapDefaultEndpoints();
app.MapOrderManagementEndpoints();

app.Run();
