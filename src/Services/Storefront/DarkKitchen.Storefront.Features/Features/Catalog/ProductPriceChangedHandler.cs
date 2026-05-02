using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Storefront.Features.Features.Catalog;

public static class ProductPriceChangedHandler
{
    public static Task Handle(IntegrationEventEnvelope<ProductPriceChanged> message, StorefrontDbContext db, CancellationToken ct)
    {
        return ApplyAsync(message.Payload, db, ct);
    }

    public static async Task ApplyAsync(ProductPriceChanged payload, StorefrontDbContext db, CancellationToken ct)
    {
        var snapshot = await db.MenuItems.FirstOrDefaultAsync(item => item.MenuItemId == payload.ProductId && item.BrandId == payload.BrandId, ct);
        if (snapshot is null)
        {
            return;
        }

        snapshot.UpdatePrice(payload.Price, payload.Currency, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(ct);
    }
}
