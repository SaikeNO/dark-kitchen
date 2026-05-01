namespace DarkKitchen.Catalog.Api.Features;

public static class PublicMenuRoutes
{
    public static IEndpointRouteBuilder MapPublicMenuEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/menu/brands/{brandId:guid}", GetBrandMenuEndpoint.HandleAsync).AllowAnonymous();
        return app;
    }
}
