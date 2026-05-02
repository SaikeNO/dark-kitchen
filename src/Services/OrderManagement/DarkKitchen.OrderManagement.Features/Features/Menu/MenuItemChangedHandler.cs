using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.Features.Features.Menu;

public static class MenuItemChangedHandler
{
    public static Task Handle(
        IntegrationEventEnvelope<MenuItemChanged> envelope,
        OrderManagementDbContext db,
        CancellationToken ct)
    {
        return UpsertAsync(envelope, db, ct);
    }

    public static async Task UpsertAsync(
        IntegrationEventEnvelope<MenuItemChanged> envelope,
        OrderManagementDbContext db,
        CancellationToken ct)
    {
        var payload = envelope.Payload;
        var snapshot = await db.MenuItemSnapshots
            .FirstOrDefaultAsync(item => item.MenuItemId == payload.ProductId, ct);

        if (snapshot is null)
        {
            db.MenuItemSnapshots.Add(MenuItemSnapshot.Create(
                payload.ProductId,
                payload.BrandId,
                payload.CategoryId,
                payload.Name,
                payload.Description,
                payload.Price,
                payload.Currency,
                payload.IsActive,
                envelope.OccurredAt));
        }
        else
        {
            snapshot.ApplyCatalogData(
                payload.BrandId,
                payload.CategoryId,
                payload.Name,
                payload.Description,
                payload.Price,
                payload.Currency,
                payload.IsActive,
                envelope.OccurredAt);
        }

        await db.SaveChangesAsync(ct);
    }
}
