using DarkKitchen.Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Inventory.IntegrationTests.Features.InventoryAdmin;

[Collection(AspireAppCollection.Name)]
public sealed class AdjustInventoryItemEndpointTests(AspireAppFixture fixture) : InventoryIntegrationTestBase(fixture)
{
    [Fact]
    public async Task AdjustsStockAndMinimumWithInventoryLog()
    {
        await using var db = await CreateDbContextAsync();
        var itemId = Guid.NewGuid();
        db.WarehouseItems.Add(WarehouseItem.Create(itemId, $"Adjust Item {NewSuffix()}", "g", DateTimeOffset.UtcNow, 10, 2));
        await db.SaveChangesAsync();

        using var api = new InventoryApiClient(await CreateInventoryClientAsync());
        var item = await api.AdjustAsync(itemId, 6, 4);

        Assert.Equal(6, item.OnHandQuantity);
        Assert.Equal(4, item.MinSafetyLevel);
        var log = await db.InventoryLogs.SingleAsync(entity => entity.WarehouseItemId == itemId);
        Assert.Equal(InventoryLogChangeType.Adjustment, log.ChangeType);
        Assert.Equal(-4, log.Amount);
    }

    [Fact]
    public async Task AdjustmentBelowReservedReturnsValidationProblem()
    {
        await using var db = await CreateDbContextAsync();
        var itemId = Guid.NewGuid();
        var item = WarehouseItem.Create(itemId, $"Reserved Item {NewSuffix()}", "g", DateTimeOffset.UtcNow, 10, 2);
        item.Reserve(5, DateTimeOffset.UtcNow);
        db.WarehouseItems.Add(item);
        await db.SaveChangesAsync();

        using var api = new InventoryApiClient(await CreateInventoryClientAsync());
        using var response = await api.PostAdjustmentAsync(itemId, new AdjustmentRequest(4, 2, null));
        var problem = await response.ReadValidationProblemAsync();

        Assert.Contains("onHandQuantity", problem.Errors.Keys);
    }
}
