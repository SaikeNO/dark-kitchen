namespace DarkKitchen.Catalog.Features.Features.ProductStationRoutes;

public static class ProductStationRouteRoutes
{
    public static IEndpointRouteBuilder MapProductStationRouteEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/admin/products/{productId:guid}/station-route",
                UpsertProductStationRouteEndpoint.HandleAsync)
            .RequireAuthorization(CatalogPolicies.Manager);

        return app;
    }
}
