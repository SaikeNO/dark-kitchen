namespace DarkKitchen.OrderManagement.Features.Features.Orders;

public static class OrderRoutes
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders/storefront", CreateStorefrontOrderEndpoint.HandleAsync);
        app.MapPost("/api/mock-delivery/webhooks/orders", CreateMockDeliveryOrderEndpoint.HandleAsync);
        app.MapGet("/api/orders/{orderId:guid}", GetOrderEndpoint.HandleAsync);

        return app;
    }
}
