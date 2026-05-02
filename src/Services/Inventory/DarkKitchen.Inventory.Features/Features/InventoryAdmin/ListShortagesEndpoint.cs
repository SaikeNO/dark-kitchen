using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Inventory.Features.Features.InventoryAdmin;

public static class ListShortagesEndpoint
{
    public static async Task<IReadOnlyList<InventoryItemResponse>> HandleAsync(
        InventoryDbContext db,
        CancellationToken ct)
    {
        var items = await db.WarehouseItems
            .AsNoTracking()
            .Where(item => item.OnHandQuantity - item.ReservedQuantity < item.MinSafetyLevel)
            .OrderBy(item => item.Name)
            .ToArrayAsync(ct);

        return items.Select(InventoryItemResponse.FromItem).ToArray();
    }
}
