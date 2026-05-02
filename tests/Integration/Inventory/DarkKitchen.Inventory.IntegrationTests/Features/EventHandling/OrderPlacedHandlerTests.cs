using DarkKitchen.Contracts.Events;
using DarkKitchen.Inventory.Domain;
using DarkKitchen.Inventory.Features.Application;
using DarkKitchen.Inventory.Features.Features.Orders;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Inventory.IntegrationTests.Features.EventHandling;

[Collection(AspireAppCollection.Name)]
public sealed class OrderPlacedHandlerTests(AspireAppFixture fixture) : InventoryIntegrationTestBase(fixture)
{
    [Fact]
    public async Task AvailableOrderPublishesInventoryReserved()
    {
        await using var db = await CreateDbContextAsync();
        var scenario = await SeedReservationScenarioAsync(db, onHandQuantity: 10, recipeQuantity: 2);
        var orderId = Guid.NewGuid();

        var result = await OrderPlacedHandler.ReserveAsync(CreateOrder(orderId, scenario.ProductId, quantity: 2), db, CancellationToken.None);

        var item = await db.WarehouseItems.AsNoTracking().SingleAsync(entity => entity.Id == scenario.IngredientId);
        Assert.Equal(4, item.ReservedQuantity);
        Assert.IsType<IntegrationEventEnvelope<InventoryReserved>>(result);
        Assert.Equal(1, await db.InventoryLogs.CountAsync(log => log.OrderId == orderId));
    }

    [Fact]
    public async Task MissingRecipePublishesReservationFailed()
    {
        await using var db = await CreateDbContextAsync();

        var result = await OrderPlacedHandler.ReserveAsync(CreateOrder(Guid.NewGuid(), Guid.NewGuid(), quantity: 1), db, CancellationToken.None);

        var failed = Assert.IsType<IntegrationEventEnvelope<InventoryReservationFailed>>(result);
        Assert.Equal(InventoryReasonCodes.RecipeMissing, failed.Payload.ReasonCode);
    }

    [Fact]
    public async Task MissingStockPublishesReservationFailed()
    {
        await using var db = await CreateDbContextAsync();
        var scenario = await SeedReservationScenarioAsync(db, onHandQuantity: 1, recipeQuantity: 2);

        var result = await OrderPlacedHandler.ReserveAsync(CreateOrder(Guid.NewGuid(), scenario.ProductId, quantity: 1), db, CancellationToken.None);

        var failed = Assert.IsType<IntegrationEventEnvelope<InventoryReservationFailed>>(result);
        Assert.Equal(InventoryReasonCodes.IngredientUnavailable, failed.Payload.ReasonCode);
    }

    [Fact]
    public async Task DuplicateOrderDoesNotDoubleReserveStock()
    {
        await using var db = await CreateDbContextAsync();
        var scenario = await SeedReservationScenarioAsync(db, onHandQuantity: 10, recipeQuantity: 2);
        var orderId = Guid.NewGuid();

        await OrderPlacedHandler.ReserveAsync(CreateOrder(orderId, scenario.ProductId, quantity: 1), db, CancellationToken.None);
        await OrderPlacedHandler.ReserveAsync(CreateOrder(orderId, scenario.ProductId, quantity: 1), db, CancellationToken.None);

        var item = await db.WarehouseItems.AsNoTracking().SingleAsync(entity => entity.Id == scenario.IngredientId);
        Assert.Equal(2, item.ReservedQuantity);
        Assert.Equal(1, await db.StockReservations.CountAsync(reservation => reservation.OrderId == orderId));
    }

    [Fact]
    public async Task ParallelOrdersDoNotOverReserveStock()
    {
        await using var setupDb = await CreateDbContextAsync();
        var scenario = await SeedReservationScenarioAsync(setupDb, onHandQuantity: 5, recipeQuantity: 4);
        var envelope1 = CreateOrder(Guid.NewGuid(), scenario.ProductId, quantity: 1);
        var envelope2 = CreateOrder(Guid.NewGuid(), scenario.ProductId, quantity: 1);

        await using var db1 = await CreateDbContextAsync();
        await using var db2 = await CreateDbContextAsync();

        await Task.WhenAll(
            OrderPlacedHandler.ReserveAsync(envelope1, db1, CancellationToken.None),
            OrderPlacedHandler.ReserveAsync(envelope2, db2, CancellationToken.None));

        await using var assertDb = await CreateDbContextAsync();
        var orderIds = new[] { envelope1.Payload.OrderId, envelope2.Payload.OrderId };
        var item = await assertDb.WarehouseItems.AsNoTracking().SingleAsync(entity => entity.Id == scenario.IngredientId);
        Assert.Equal(4, item.ReservedQuantity);
        Assert.Equal(1, await assertDb.StockReservations.CountAsync(reservation => orderIds.Contains(reservation.OrderId) && reservation.Status == StockReservationStatus.Reserved));
        Assert.Equal(1, await assertDb.StockReservations.CountAsync(reservation => orderIds.Contains(reservation.OrderId) && reservation.Status == StockReservationStatus.Failed));
    }

    private async Task<ReservationScenario> SeedReservationScenarioAsync(
        InventoryDbContext db,
        decimal onHandQuantity,
        decimal recipeQuantity)
    {
        var now = DateTimeOffset.UtcNow;
        var ingredientId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        db.WarehouseItems.Add(WarehouseItem.Create(ingredientId, $"Reserve Item {NewSuffix()}", "g", now, onHandQuantity));
        var recipe = RecipeSnapshot.Create(productId, Guid.NewGuid(), now);
        recipe.ReplaceItems(
            Guid.NewGuid(),
            [RecipeSnapshotItem.Create(productId, ingredientId, "Reserve ingredient", "g", recipeQuantity)],
            now);
        db.RecipeSnapshots.Add(recipe);
        await db.SaveChangesAsync();

        return new ReservationScenario(productId, ingredientId);
    }

    private IntegrationEventEnvelope<OrderPlaced> CreateOrder(Guid orderId, Guid productId, int quantity)
    {
        return Envelope(new OrderPlaced(
            orderId,
            "test",
            [new OrderPlacedLine(Guid.NewGuid(), productId, quantity)]));
    }

    private sealed record ReservationScenario(Guid ProductId, Guid IngredientId);
}
