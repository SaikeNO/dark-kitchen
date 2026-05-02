using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.Features.Features.Menu;

public static class ProductPriceChangedHandler
{
    public static Task Handle(
        IntegrationEventEnvelope<ProductPriceChanged> envelope,
        OrderManagementDbContext db,
        CancellationToken ct)
    {
        return ApplyAsync(envelope, db, ct);
    }

    public static async Task ApplyAsync(
        IntegrationEventEnvelope<ProductPriceChanged> envelope,
        OrderManagementDbContext db,
        CancellationToken ct)
    {
        var payload = envelope.Payload;
        var snapshot = await db.MenuItemSnapshots
            .FirstOrDefaultAsync(item => item.MenuItemId == payload.ProductId && item.BrandId == payload.BrandId, ct);

        if (snapshot is null)
        {
            return;
        }

        snapshot.ChangePrice(payload.Price, payload.Currency, envelope.OccurredAt);
        await db.SaveChangesAsync(ct);
    }
}
