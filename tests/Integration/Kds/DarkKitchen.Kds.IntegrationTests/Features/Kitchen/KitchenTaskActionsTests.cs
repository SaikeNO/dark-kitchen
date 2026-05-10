using DarkKitchen.Contracts.Events;
using DarkKitchen.Kds.Domain;
using DarkKitchen.Kds.Features.Features.Catalog;
using DarkKitchen.Kds.Features.Features.Kitchen;
using DarkKitchen.Kds.Features.Features.Orders;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Kds.IntegrationTests.Features.Kitchen;

[Collection(AspireAppCollection.Name)]
public sealed class KitchenTaskActionsTests(AspireAppFixture fixture) : KdsIntegrationTestBase(fixture)
{
    [Fact]
    public async Task StartAndDone_ReturnEventsOnlyOnFirstTransition()
    {
        await using var db = await CreateDbContextAsync();
        var taskId = await SeedPendingTaskAsync(db);

        var started = await KitchenTaskActions.StartAsync(taskId, db, CancellationToken.None);
        var duplicateStart = await KitchenTaskActions.StartAsync(taskId, db, CancellationToken.None);
        var completed = await KitchenTaskActions.CompleteAsync(taskId, db, CancellationToken.None);
        var duplicateComplete = await KitchenTaskActions.CompleteAsync(taskId, db, CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.IsType<IntegrationEventEnvelope<ItemPreparationStarted>>(started.IntegrationEvent);
        Assert.Null(duplicateStart.IntegrationEvent);
        Assert.IsType<IntegrationEventEnvelope<ItemPreparationCompleted>>(completed.IntegrationEvent);
        Assert.Null(duplicateComplete.IntegrationEvent);
        Assert.Equal("Done", duplicateComplete.Response!.Status);
    }

    [Fact]
    public async Task DoneBeforeStart_ReturnsConflict()
    {
        await using var db = await CreateDbContextAsync();
        var taskId = await SeedPendingTaskAsync(db);

        var result = await KitchenTaskActions.CompleteAsync(taskId, db, CancellationToken.None);

        Assert.Equal(KitchenTaskActionError.Conflict, result.Error);
        Assert.Null(result.IntegrationEvent);
    }

    [Fact]
    public async Task RoutingMissingTask_CannotStart()
    {
        await using var db = await CreateDbContextAsync();
        var brandId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await OrderAcceptedHandler.CreateTicketAsync(
            Envelope(new OrderAccepted(
                Guid.NewGuid(),
                "test",
                [new OrderAcceptedLine(Guid.NewGuid(), productId, "Missing", 1)]), brandId.ToString("D")),
            db,
            CancellationToken.None);
        var taskId = await db.KitchenTasks
            .Where(task => task.MenuItemId == productId && task.Status == KitchenTaskStatus.RoutingMissing)
            .Select(task => task.Id)
            .SingleAsync();

        var result = await KitchenTaskActions.StartAsync(taskId, db, CancellationToken.None);

        Assert.Equal(KitchenTaskActionError.Conflict, result.Error);
    }

    private async Task<Guid> SeedPendingTaskAsync(KdsDbContext db)
    {
        var brandId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var stationId = Guid.NewGuid();
        await StationChangedHandler.Handle(
            Envelope(new StationChanged(stationId, $"ST{NewSuffix()[..4]}", "Station", "#2f7d57", true), "catalog-global"),
            db,
            CancellationToken.None);
        await ProductStationRoutingChangedHandler.Handle(
            Envelope(new ProductStationRoutingChanged(productId, brandId, stationId, "ST"), brandId.ToString("D")),
            db,
            CancellationToken.None);
        await OrderAcceptedHandler.CreateTicketAsync(
            Envelope(new OrderAccepted(
                Guid.NewGuid(),
                "test",
                [new OrderAcceptedLine(Guid.NewGuid(), productId, "Burger", 1)]), brandId.ToString("D")),
            db,
            CancellationToken.None);

        return await db.KitchenTasks
            .Where(task => task.MenuItemId == productId)
            .Select(task => task.Id)
            .SingleAsync();
    }
}
