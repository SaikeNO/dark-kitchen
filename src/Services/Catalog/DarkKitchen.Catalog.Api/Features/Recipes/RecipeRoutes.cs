namespace DarkKitchen.Catalog.Api.Features;

public static class RecipeRoutes
{
    public static IEndpointRouteBuilder MapRecipeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/products/{productId:guid}/recipe")
            .RequireAuthorization(CatalogPolicies.Operator);

        group.MapGet("/", GetRecipeEndpoint.HandleAsync);
        group.MapPut("/", UpsertRecipeEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Manager);

        return app;
    }
}
