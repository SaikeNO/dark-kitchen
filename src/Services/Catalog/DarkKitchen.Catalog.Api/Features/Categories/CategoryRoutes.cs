namespace DarkKitchen.Catalog.Api.Features;

public static class CategoryRoutes
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/categories")
            .RequireAuthorization(CatalogPolicies.Operator);

        group.MapGet("/", ListCategoriesEndpoint.HandleAsync);
        group.MapPost("/", CreateCategoryEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Manager);
        group.MapPut("/{categoryId:guid}", UpdateCategoryEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Manager);
        group.MapPost("/{categoryId:guid}/deactivate", DeactivateCategoryEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Manager);

        return app;
    }
}
