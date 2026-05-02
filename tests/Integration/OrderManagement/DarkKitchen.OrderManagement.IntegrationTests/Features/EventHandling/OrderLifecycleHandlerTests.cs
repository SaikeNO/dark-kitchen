using DarkKitchen.Contracts.Events;
using DarkKitchen.OrderManagement.Domain;
using DarkKitchen.OrderManagement.Features.Features.Inventory;
using DarkKitchen.OrderManagement.Features.Features.Menu;
using DarkKitchen.OrderManagement.Features.Features.Progress;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.IntegrationTests.Features.EventHandling;

[Collection(AspireAppCollection.Name)]
public sealed class OrderLifecycleHandlerTests(AspireAppFixture fixture) : OrderManagementIntegrationTestBase(fixture)
{
    [Fact]
    public async Task InventoryReservedAcceptsOrderAndCreatesOrderAccepted()
    {
        await using var db = await CreateDbContextAsync();
        var brandId = Guid.NewGuid();
        var order = await SeedOrderAsync(db, brandId);
        var correlationId = Guid.NewGuid();

        var accepted = await InventoryReservedHandler.AcceptAsync(
            Envelope(new InventoryReserved(order.Id, Guid.NewGuid()), correlationId, brandId.ToString("D")),
            db,
            CancellationToken.None);

        Assert.NotNull(accepted);
        Assert.Equal(correlationId, accepted.CorrelationId);
        Assert.Equal(order.Id, accepted.Payload.OrderId);
        Assert.Equal("storefront", accepted.Payload.SourceChannel);
        Assert.Single(accepted.Payload.Items);

        db.ChangeTracker.Clear();
        var saved = await db.Orders.Include(entity => entity.History).SingleAsync(entity => entity.Id == order.Id);
        Assert.Equal(OrderStatus.Accepted, saved.Status);
        Assert.Equal(2, saved.History.Count);
    }

    [Fact]
    public async Task InventoryReservationFailedRejectsOrderWithoutAcceptedEvent()
    {
        await using var db = await CreateDbContextAsync();
        var order = await SeedOrderAsync(db, Guid.NewGuid());

        await InventoryReservationFailedHandler.RejectAsync(
            Envelope(new InventoryReservationFailed(order.Id, "ingredient_unavailable")),
            db,
            CancellationToken.None);

        db.ChangeTracker.Clear();
        var saved = await db.Orders.Include(entity => entity.History).SingleAsync(entity => entity.Id == order.Id);
        Assert.Equal(OrderStatus.Rejected, saved.Status);
        Assert.Equal(2, saved.History.Count);
        Assert.Contains(saved.History, history => history.Reason == "ingredient_unavailable");
    }

    [Fact]
    public async Task ProgressEventsMoveStatusAndAppendHistory()
    {
        await using var db = await CreateDbContextAsync();
        var order = await SeedOrderAsync(db, Guid.NewGuid());
        var orderItemId = order.Items[0].Id;
        await InventoryReservedHandler.AcceptAsync(
            Envelope(new InventoryReserved(order.Id, Guid.NewGuid())),
            db,
            CancellationToken.None);
        db.ChangeTracker.Clear();

        await ItemPreparationStartedHandler.Handle(
            Envelope(new ItemPreparationStarted(order.Id, orderItemId, "GRILL")),
            db,
            CancellationToken.None);
        await OrderReadyForPackingHandler.Handle(
            Envelope(new OrderReadyForPacking(order.Id)),
            db,
            CancellationToken.None);
        await OrderReadyForPickupHandler.Handle(
            Envelope(new OrderReadyForPickup(order.Id, "A-42")),
            db,
            CancellationToken.None);

        db.ChangeTracker.Clear();
        var saved = await db.Orders.Include(entity => entity.History).SingleAsync(entity => entity.Id == order.Id);
        Assert.Equal(OrderStatus.ReadyForPickup, saved.Status);
        Assert.Equal(5, saved.History.Count);
    }

    [Fact]
    public async Task MenuEventsMaintainMenuReadModel()
    {
        await using var db = await CreateDbContextAsync();
        var brandId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        await MenuItemChangedHandler.UpsertAsync(
            Envelope(new MenuItemChanged(productId, brandId, Guid.NewGuid(), "Burger", null, 10m, "PLN", true)),
            db,
            CancellationToken.None);
        await ProductPriceChangedHandler.ApplyAsync(
            Envelope(new ProductPriceChanged(productId, brandId, 12.50m, "PLN")),
            db,
            CancellationToken.None);

        var snapshot = await db.MenuItemSnapshots.SingleAsync(entity => entity.MenuItemId == productId);
        Assert.Equal("Burger", snapshot.Name);
        Assert.Equal(12.50m, snapshot.Price);
        Assert.True(snapshot.IsActive);
    }
}
