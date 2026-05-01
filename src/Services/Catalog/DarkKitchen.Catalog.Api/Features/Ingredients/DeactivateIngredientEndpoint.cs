using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Api.Features;

public static class DeactivateIngredientEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid ingredientId,
        IDbContextOutbox<CatalogDbContext> outbox,
        CancellationToken ct)
    {
        var ingredient = await outbox.DbContext.Ingredients.FirstOrDefaultAsync(entity => entity.Id == ingredientId, ct);
        if (ingredient is null)
        {
            return Results.NotFound();
        }

        ingredient.IsActive = false;
        ingredient.UpdatedAt = DateTimeOffset.UtcNow;
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Ok(new Response(ingredient.Id, ingredient.Name, ingredient.Unit, ingredient.IsActive));
    }

    public sealed record Response(Guid Id, string Name, string Unit, bool IsActive);
}
