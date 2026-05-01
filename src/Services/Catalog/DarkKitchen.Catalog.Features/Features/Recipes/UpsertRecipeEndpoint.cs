using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.Recipes;

public static class UpsertRecipeEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid productId,
        Request request,
        IDbContextOutbox<CatalogDbContext> outbox,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var db = outbox.DbContext;
        var product = await db.Products.FirstOrDefaultAsync(entity => entity.Id == productId, ct);
        if (product is null)
        {
            return Results.NotFound();
        }

        var validation = await ValidateAsync(request, db, ct);
        if (validation is not null)
        {
            return validation;
        }

        var existingRecipe = await db.Recipes
            .Include(recipe => recipe.Items)
            .FirstOrDefaultAsync(recipe => recipe.ProductId == productId, ct);

        if (existingRecipe is null)
        {
            existingRecipe = Recipe.Create(productId, DateTimeOffset.UtcNow);
            db.Recipes.Add(existingRecipe);
        }

        db.RecipeItems.RemoveRange(existingRecipe.Items);
        var now = DateTimeOffset.UtcNow;
        var recipeItems = request.Items
            .Select(item => RecipeItem.Create(productId, item.IngredientId, item.Quantity));
        existingRecipe.ReplaceItems(recipeItems, now);
        product.Touch(now);

        var ingredientIds = request.Items.Select(item => item.IngredientId).ToArray();
        var ingredients = await db.Ingredients
            .AsNoTracking()
            .Where(ingredient => ingredientIds.Contains(ingredient.Id))
            .ToDictionaryAsync(ingredient => ingredient.Id, ct);
        var eventItems = request.Items
            .Select(item =>
            {
                var ingredient = ingredients[item.IngredientId];
                return new RecipeChangedItem(item.IngredientId, ingredient.Name, ingredient.Unit, item.Quantity);
            })
            .ToArray();

        await outbox.PublishAsync(CatalogEventFactory.RecipeChanged(product, eventItems, httpContext));
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Ok(new Response(productId, await LoadItemsAsync(productId, db, ct)));
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

    private static async Task<IResult?> ValidateAsync(Request request, CatalogDbContext db, CancellationToken ct)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            return ApiValidation.Problem(("items", "Recipe must contain at least one ingredient."));
        }

        if (request.Items.Any(item => item.IngredientId == Guid.Empty || item.Quantity <= 0))
        {
            return ApiValidation.Problem(("items", "Every recipe item must have an ingredient and positive quantity."));
        }

        var distinctIngredientIds = request.Items.Select(item => item.IngredientId).Distinct().ToArray();
        if (distinctIngredientIds.Length != request.Items.Count)
        {
            return ApiValidation.Problem(("items", "Recipe cannot contain duplicate ingredients."));
        }

        var activeIngredientCount = await db.Ingredients
            .CountAsync(ingredient => distinctIngredientIds.Contains(ingredient.Id) && ingredient.IsActive, ct);

        return activeIngredientCount == distinctIngredientIds.Length
            ? null
            : ApiValidation.Problem(("items", "Every recipe ingredient must exist and be active."));
    }

    public sealed record Request(IReadOnlyList<ItemRequest> Items);

    public sealed record ItemRequest(Guid IngredientId, decimal Quantity);

    public sealed record Response(
        Guid ProductId,
        IReadOnlyList<ItemResponse> Items);

    public sealed record ItemResponse(
        Guid IngredientId,
        string IngredientName,
        string Unit,
        decimal Quantity);
}
