namespace DarkKitchen.Catalog.Features.Features.Ingredients;

public static class IngredientRoutes
{
    public static IEndpointRouteBuilder MapIngredientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/ingredients")
            .RequireAuthorization(CatalogPolicies.Operator);

        group.MapGet("/", ListIngredientsEndpoint.HandleAsync);
        group.MapPost("/", CreateIngredientEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Manager);
        group.MapPut("/{ingredientId:guid}", UpdateIngredientEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Manager);
        group.MapPost("/{ingredientId:guid}/deactivate", DeactivateIngredientEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Manager);

        return app;
    }
}
