using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Storefront.Features.Features.Catalog;

public static class MenuItemChangedHandler
{
    public static Task Handle(IntegrationEventEnvelope<MenuItemChanged> message, StorefrontDbContext db, CancellationToken ct)
    {
        return UpsertAsync(message.Payload, db, ct);
    }

    public static async Task UpsertAsync(MenuItemChanged payload, StorefrontDbContext db, CancellationToken ct)
    {
        var snapshot = await db.MenuItems.FirstOrDefaultAsync(item => item.MenuItemId == payload.ProductId, ct);
        var now = DateTimeOffset.UtcNow;
        if (snapshot is null)
        {
            db.MenuItems.Add(MenuItemSnapshot.Create(
                payload.ProductId,
                payload.BrandId,
                payload.CategoryId,
                payload.Name,
                payload.Description,
                payload.ImageUrl,
                payload.Price,
                payload.Currency,
                payload.IsActive,
                now));
        }
        else
        {
            snapshot.Update(
                payload.BrandId,
                payload.CategoryId,
                payload.Name,
                payload.Description,
                payload.ImageUrl,
                payload.Price,
                payload.Currency,
                payload.IsActive,
                now);
        }

        await db.SaveChangesAsync(ct);
    }
}
