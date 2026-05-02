namespace DarkKitchen.Storefront.Features.Features.Menu;

public static class MenuRoutes
{
    public static IEndpointRouteBuilder MapStorefrontMenuEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/storefront/context", GetStorefrontContextEndpoint.HandleAsync);
        app.MapGet("/api/storefront/menu", GetStorefrontMenuEndpoint.HandleAsync);

        return app;
    }
}
