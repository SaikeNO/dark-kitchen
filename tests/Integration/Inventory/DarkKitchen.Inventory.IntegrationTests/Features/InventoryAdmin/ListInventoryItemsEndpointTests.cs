using DarkKitchen.Inventory.Domain;

namespace DarkKitchen.Inventory.IntegrationTests.Features.InventoryAdmin;

[Collection(AspireAppCollection.Name)]
public sealed class ListInventoryItemsEndpointTests(AspireAppFixture fixture) : InventoryIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ReturnsWarehouseItems()
    {
        await using var db = await CreateDbContextAsync();
        var itemId = Guid.NewGuid();
        db.WarehouseItems.Add(WarehouseItem.Create(itemId, $"List Item {NewSuffix()}", "g", DateTimeOffset.UtcNow, 5, 2));
        await db.SaveChangesAsync();

        using var api = new InventoryApiClient(await CreateInventoryClientAsync());
        var items = await api.ListItemsAsync();

        var item = Assert.Single(items, item => item.IngredientId == itemId);
        Assert.Equal(5, item.OnHandQuantity);
        Assert.Equal(5, item.AvailableQuantity);
    }
}
