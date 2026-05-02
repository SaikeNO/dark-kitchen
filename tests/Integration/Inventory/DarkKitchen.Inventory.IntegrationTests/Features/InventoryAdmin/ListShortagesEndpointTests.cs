using DarkKitchen.Inventory.Domain;

namespace DarkKitchen.Inventory.IntegrationTests.Features.InventoryAdmin;

[Collection(AspireAppCollection.Name)]
public sealed class ListShortagesEndpointTests(AspireAppFixture fixture) : InventoryIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ReturnsOnlyItemsBelowMinimum()
    {
        await using var db = await CreateDbContextAsync();
        var suffix = NewSuffix();
        var shortageId = Guid.NewGuid();
        var healthyId = Guid.NewGuid();
        db.WarehouseItems.AddRange(
            WarehouseItem.Create(shortageId, $"Shortage {suffix}", "szt", DateTimeOffset.UtcNow, 2, 5),
            WarehouseItem.Create(healthyId, $"Healthy {suffix}", "szt", DateTimeOffset.UtcNow, 10, 5));
        await db.SaveChangesAsync();

        using var api = new InventoryApiClient(await CreateInventoryClientAsync());
        var shortages = await api.ListShortagesAsync();

        Assert.Contains(shortages, item => item.IngredientId == shortageId && item.ReorderQuantity == 3);
        Assert.DoesNotContain(shortages, item => item.IngredientId == healthyId);
    }
}
