namespace DarkKitchen.Catalog.Api.Features;

public static class StationRoutes
{
    public static IEndpointRouteBuilder MapStationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/stations")
            .RequireAuthorization(CatalogPolicies.Operator);

        group.MapGet("/", ListStationsEndpoint.HandleAsync);
        group.MapPost("/", CreateStationEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Manager);
        group.MapPut("/{stationId:guid}", UpdateStationEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Manager);
        group.MapPost("/{stationId:guid}/deactivate", DeactivateStationEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Manager);

        return app;
    }
}
