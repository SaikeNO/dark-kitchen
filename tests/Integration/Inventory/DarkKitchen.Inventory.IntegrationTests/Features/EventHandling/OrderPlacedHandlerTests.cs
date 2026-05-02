using DarkKitchen.Contracts.Events;
using DarkKitchen.Inventory.Domain;
using DarkKitchen.Inventory.Features.Application;
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
        var outbox = new FakeInventoryOutbox(db);
        var service = new InventoryReservationService(outbox);
        var orderId = Guid.NewGuid();

        await service.HandleOrderPlacedAsync(CreateOrder(orderId, scenario.ProductId, quantity: 2), CancellationToken.None);

        var item = await db.WarehouseItems.AsNoTracking().SingleAsync(entity => entity.Id == scenario.IngredientId);
        Assert.Equal(4, item.ReservedQuantity);
        Assert.Contains(outbox.Published, message => message is IntegrationEventEnvelope<InventoryReserved>);
        Assert.Equal(1, await db.InventoryLogs.CountAsync(log => log.OrderId == orderId));
    }

    [Fact]
    public async Task MissingRecipePublishesReservationFailed()
    {
        await using var db = await CreateDbContextAsync();
        var outbox = new FakeInventoryOutbox(db);
        var service = new InventoryReservationService(outbox);

        await service.HandleOrderPlacedAsync(CreateOrder(Guid.NewGuid(), Guid.NewGuid(), quantity: 1), CancellationToken.None);

        var failed = Assert.IsType<IntegrationEventEnvelope<InventoryReservationFailed>>(Assert.Single(outbox.Published));
        Assert.Equal(InventoryReasonCodes.RecipeMissing, failed.Payload.ReasonCode);
    }

    [Fact]
    public async Task MissingStockPublishesReservationFailed()
    {
        await using var db = await CreateDbContextAsync();
        var scenario = await SeedReservationScenarioAsync(db, onHandQuantity: 1, recipeQuantity: 2);
        var outbox = new FakeInventoryOutbox(db);
        var service = new InventoryReservationService(outbox);

        await service.HandleOrderPlacedAsync(CreateOrder(Guid.NewGuid(), scenario.ProductId, quantity: 1), CancellationToken.None);

        var failed = Assert.IsType<IntegrationEventEnvelope<InventoryReservationFailed>>(Assert.Single(outbox.Published));
        Assert.Equal(InventoryReasonCodes.IngredientUnavailable, failed.Payload.ReasonCode);
    }

    [Fact]
    public async Task DuplicateOrderDoesNotDoubleReserveStock()
    {
        await using var db = await CreateDbContextAsync();
        var scenario = await SeedReservationScenarioAsync(db, onHandQuantity: 10, recipeQuantity: 2);
        var outbox = new FakeInventoryOutbox(db);
        var service = new InventoryReservationService(outbox);
        var orderId = Guid.NewGuid();

        await service.HandleOrderPlacedAsync(CreateOrder(orderId, scenario.ProductId, quantity: 1), CancellationToken.None);
        await service.HandleOrderPlacedAsync(CreateOrder(orderId, scenario.ProductId, quantity: 1), CancellationToken.None);

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
        var outbox1 = new FakeInventoryOutbox(db1);
        var outbox2 = new FakeInventoryOutbox(db2);
        var service1 = new InventoryReservationService(outbox1);
        var service2 = new InventoryReservationService(outbox2);

        await Task.WhenAll(
            service1.HandleOrderPlacedAsync(envelope1, CancellationToken.None),
            service2.HandleOrderPlacedAsync(envelope2, CancellationToken.None));

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
