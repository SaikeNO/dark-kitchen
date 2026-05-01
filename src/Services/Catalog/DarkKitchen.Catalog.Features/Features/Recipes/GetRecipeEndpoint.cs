using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.Recipes;

public static class GetRecipeEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid productId,
        CatalogDbContext db,
        CancellationToken ct)
    {
        var productExists = await db.Products.AnyAsync(product => product.Id == productId, ct);
        return productExists
            ? Results.Ok(new Response(productId, await LoadItemsAsync(productId, db, ct)))
            : Results.NotFound();
    }

    private static async Task<IReadOnlyList<ItemResponse>> LoadItemsAsync(
        Guid productId,
        CatalogDbContext db,
        CancellationToken ct)
    {
        return await db.RecipeItems
            .AsNoTracking()
            .Where(item => item.ProductId == productId)
            .Include(item => item.Ingredient)
            .OrderBy(item => item.Ingredient!.Name)
            .Select(item => new ItemResponse(
                item.IngredientId,
                item.Ingredient!.Name,
                item.Ingredient.Unit,
                item.Quantity))
            .ToArrayAsync(ct);
    }

    public sealed record Response(
        Guid ProductId,
        IReadOnlyList<ItemResponse> Items);

    public sealed record ItemResponse(
        Guid IngredientId,
        string IngredientName,
        string Unit,
        decimal Quantity);
}
