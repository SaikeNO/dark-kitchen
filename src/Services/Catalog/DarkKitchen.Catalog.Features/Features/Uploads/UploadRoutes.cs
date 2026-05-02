namespace DarkKitchen.Catalog.Features.Features.Uploads;

public static class UploadRoutes
{
    public static IEndpointRouteBuilder MapUploadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/uploads")
            .RequireAuthorization(CatalogPolicies.Manager);

        group.MapPost("/{kind}", UploadAssetEndpoint.HandleAsync)
            .DisableAntiforgery();

        return app;
    }
}
