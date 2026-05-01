namespace DarkKitchen.Catalog.Features.Features.Brands;

public static class BrandRoutes
{
    public static IEndpointRouteBuilder MapBrandEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/brands")
            .RequireAuthorization(CatalogPolicies.Operator);

        group.MapGet("/", ListBrandsEndpoint.HandleAsync);
        group.MapPost("/", CreateBrandEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Manager);
        group.MapPut("/{brandId:guid}", UpdateBrandEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Manager);
        group.MapPost("/{brandId:guid}/deactivate", DeactivateBrandEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Manager);

        return app;
    }
}
