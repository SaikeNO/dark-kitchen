using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Inventory.Features.Features.InventoryAdmin;

public static class ListInventoryItemsEndpoint
{
    public static async Task<IReadOnlyList<InventoryItemResponse>> HandleAsync(
        InventoryDbContext db,
        CancellationToken ct)
    {
        var items = await db.WarehouseItems
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .ToArrayAsync(ct);

        return items.Select(InventoryItemResponse.FromItem).ToArray();
    }
}
