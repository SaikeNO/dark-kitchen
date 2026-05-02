using DarkKitchen.Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Inventory.IntegrationTests.Features.InventoryAdmin;

[Collection(AspireAppCollection.Name)]
public sealed class RecordDeliveryEndpointTests(AspireAppFixture fixture) : InventoryIntegrationTestBase(fixture)
{
    [Fact]
    public async Task RecordsDeliveryAndInventoryLog()
    {
        await using var db = await CreateDbContextAsync();
        var itemId = Guid.NewGuid();
        db.WarehouseItems.Add(WarehouseItem.Create(itemId, $"Delivery Item {NewSuffix()}", "kg", DateTimeOffset.UtcNow, 4, 1));
        await db.SaveChangesAsync();

        using var api = new InventoryApiClient(await CreateInventoryClientAsync());
        var item = await api.RecordDeliveryAsync(itemId, 3);

        Assert.Equal(7, item.OnHandQuantity);
        await db.Entry(await db.WarehouseItems.SingleAsync(entity => entity.Id == itemId)).ReloadAsync();
        var log = await db.InventoryLogs.SingleAsync(entity => entity.WarehouseItemId == itemId);
        Assert.Equal(InventoryLogChangeType.Delivery, log.ChangeType);
        Assert.Equal(3, log.Amount);
    }

    [Fact]
    public async Task NonPositiveDeliveryReturnsValidationProblem()
    {
        await using var db = await CreateDbContextAsync();
        var itemId = Guid.NewGuid();
        db.WarehouseItems.Add(WarehouseItem.Create(itemId, $"Invalid Delivery {NewSuffix()}", "kg", DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();

        using var api = new InventoryApiClient(await CreateInventoryClientAsync());
        using var response = await api.PostDeliveryAsync(itemId, new DeliveryRequest(0, null));
        var problem = await response.ReadValidationProblemAsync();

        Assert.Contains("quantity", problem.Errors.Keys);
    }
}
