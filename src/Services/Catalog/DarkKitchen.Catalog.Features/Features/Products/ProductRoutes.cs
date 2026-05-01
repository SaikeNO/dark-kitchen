namespace DarkKitchen.Catalog.Features.Features.Products;

public static class ProductRoutes
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/products")
            .RequireAuthorization(CatalogPolicies.Operator);

        group.MapGet("/", ListProductsEndpoint.HandleAsync);
        group.MapPost("/", CreateProductEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Manager);
        group.MapPut("/{productId:guid}", UpdateProductEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Manager);
        group.MapPost("/{productId:guid}/activate", ActivateProductEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Manager);
        group.MapPost("/{productId:guid}/deactivate", DeactivateProductEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Manager);

        return app;
    }
}
