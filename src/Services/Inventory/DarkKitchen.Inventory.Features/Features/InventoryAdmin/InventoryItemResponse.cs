namespace DarkKitchen.Inventory.Features.Features.InventoryAdmin;

public sealed record InventoryItemResponse(
    Guid IngredientId,
    string Name,
    string Unit,
    decimal OnHandQuantity,
    decimal ReservedQuantity,
    decimal AvailableQuantity,
    decimal MinSafetyLevel,
    bool IsBelowSafetyLevel,
    decimal ReorderQuantity)
{
    public static InventoryItemResponse FromItem(WarehouseItem item)
    {
        return new InventoryItemResponse(
            item.Id,
            item.Name,
            item.Unit,
            item.OnHandQuantity,
            item.ReservedQuantity,
            item.AvailableQuantity,
            item.MinSafetyLevel,
            item.IsBelowSafetyLevel,
            item.ReorderQuantity);
    }
}
