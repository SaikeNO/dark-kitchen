using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Storefront.Features.Features.Catalog;

public static class CategoryChangedHandler
{
    public static Task Handle(IntegrationEventEnvelope<CategoryChanged> message, StorefrontDbContext db, CancellationToken ct)
    {
        return UpsertAsync(message.Payload, db, ct);
    }

    public static async Task UpsertAsync(CategoryChanged payload, StorefrontDbContext db, CancellationToken ct)
    {
        var snapshot = await db.MenuCategories.FirstOrDefaultAsync(category => category.CategoryId == payload.CategoryId, ct);
        var now = DateTimeOffset.UtcNow;
        if (snapshot is null)
        {
            db.MenuCategories.Add(MenuCategorySnapshot.Create(
                payload.CategoryId,
                payload.BrandId,
                payload.Name,
                payload.SortOrder,
                payload.IsActive,
                now));
        }
        else
        {
            snapshot.Update(payload.BrandId, payload.Name, payload.SortOrder, payload.IsActive, now);
        }

        await db.SaveChangesAsync(ct);
    }
}
