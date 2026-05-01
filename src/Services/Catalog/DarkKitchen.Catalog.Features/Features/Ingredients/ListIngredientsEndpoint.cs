using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.Ingredients;

public static class ListIngredientsEndpoint
{
    public static async Task<IReadOnlyList<Response>> HandleAsync(CatalogDbContext db, CancellationToken ct)
    {
        return await db.Ingredients
            .AsNoTracking()
            .OrderBy(ingredient => ingredient.Name)
            .Select(ingredient => new Response(ingredient.Id, ingredient.Name, ingredient.Unit, ingredient.IsActive))
            .ToArrayAsync(ct);
    }

    public sealed record Response(Guid Id, string Name, string Unit, bool IsActive);
}
