using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Inventory.Features.Handlers;

public static class RecipeChangedHandler
{
    public static async Task Handle(
        IntegrationEventEnvelope<RecipeChanged> envelope,
        InventoryDbContext db,
        CancellationToken ct)
    {
        var payload = envelope.Payload;
        var now = DateTimeOffset.UtcNow;
        var snapshot = await db.RecipeSnapshots
            .Include(recipe => recipe.Items)
            .FirstOrDefaultAsync(recipe => recipe.ProductId == payload.ProductId, ct);

        if (snapshot is null)
        {
            snapshot = RecipeSnapshot.Create(payload.ProductId, payload.BrandId, now);
            db.RecipeSnapshots.Add(snapshot);
        }

        db.RecipeSnapshotItems.RemoveRange(snapshot.Items);
        snapshot.ReplaceItems(
            payload.BrandId,
            payload.Items.Select(item => RecipeSnapshotItem.Create(
                payload.ProductId,
                item.IngredientId,
                item.Name,
                item.Unit,
                item.Quantity)),
            now);

        foreach (var item in payload.Items)
        {
            var warehouseItem = await db.WarehouseItems.FirstOrDefaultAsync(entity => entity.Id == item.IngredientId, ct);
            if (warehouseItem is null)
            {
                db.WarehouseItems.Add(WarehouseItem.Create(item.IngredientId, item.Name, item.Unit, now));
            }
            else
            {
                warehouseItem.UpdateCatalogData(item.Name, item.Unit, now);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
