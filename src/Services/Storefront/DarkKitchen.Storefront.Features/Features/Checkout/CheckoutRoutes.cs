namespace DarkKitchen.Storefront.Features.Features.Checkout;

public static class CheckoutRoutes
{
    public static IEndpointRouteBuilder MapStorefrontCheckoutEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/storefront/checkout", CheckoutEndpoint.HandleAsync);
        return app;
    }
}
