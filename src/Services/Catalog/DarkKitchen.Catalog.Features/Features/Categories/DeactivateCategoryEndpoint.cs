using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.Categories;

public static class DeactivateCategoryEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid categoryId,
        IDbContextOutbox<CatalogDbContext> outbox,
        CancellationToken ct)
    {
        var category = await outbox.DbContext.Categories.FirstOrDefaultAsync(entity => entity.Id == categoryId, ct);
        if (category is null)
        {
            return Results.NotFound();
        }

        category.Deactivate(DateTimeOffset.UtcNow);
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Ok(new Response(category.Id, category.BrandId, category.Name, category.SortOrder, category.IsActive));
    }

    public sealed record Response(Guid Id, Guid BrandId, string Name, int SortOrder, bool IsActive);
}
