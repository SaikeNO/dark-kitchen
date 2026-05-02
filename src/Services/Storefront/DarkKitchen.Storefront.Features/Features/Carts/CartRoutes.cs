namespace DarkKitchen.Storefront.Features.Features.Carts;

public static class CartRoutes
{
    public static IEndpointRouteBuilder MapStorefrontCartEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/storefront/carts");

        group.MapPost("/", CreateCartEndpoint.HandleAsync);
        group.MapGet("/{cartId:guid}", GetCartEndpoint.HandleAsync);
        group.MapPatch("/{cartId:guid}", UpdateCartEndpoint.HandleAsync);

        return app;
    }
}
