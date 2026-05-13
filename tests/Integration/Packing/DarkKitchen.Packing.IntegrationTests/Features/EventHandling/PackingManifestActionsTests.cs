using DarkKitchen.Packing.Domain;
using DarkKitchen.Packing.Features.Application;
using DarkKitchen.Packing.Features.Features.Packing;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Packing.IntegrationTests.Features.EventHandling;

[Collection(AspireAppCollection.Name)]
public sealed class PackingManifestActionsTests(AspireAppFixture fixture) : PackingIntegrationTestBase(fixture)
{
    [Fact]
    public async Task OrderAccepted_CreatesManifestWithExpectedItems()
    {
        await using var db = await CreateDbContextAsync();
        var order = CreateOrder(lineCount: 2);

        var result = await PackingManifestActions.CreateManifestAsync(order, db, CancellationToken.None);
        await db.SaveChangesAsync();

        var manifest = await db.PackingManifests
            .AsNoTracking()
            .Include(entity => entity.Items)
            .SingleAsync(entity => entity.OrderId == order.Payload.OrderId);
        Assert.Equal(PackingActionError.None, result.Error);
        Assert.Equal(2, manifest.Items.Count);
        Assert.Equal(PackingManifestStatus.Waiting, manifest.Status);
    }

    [Fact]
    public async Task ItemPreparationCompleted_CompletesManifestOnlyAfterLastItem()
    {
        await using var db = await CreateDbContextAsync();
        var order = CreateOrder(lineCount: 3);
        await PackingManifestActions.CreateManifestAsync(order, db, CancellationToken.None);
        await db.SaveChangesAsync();

        object? lastEvent = null;
        foreach (var line in order.Payload.Items)
        {
            var result = await PackingManifestActions.CompleteItemAsync(
                Envelope(new ItemPreparationCompleted(order.Payload.OrderId, line.OrderItemId, "HOT")),
                db,
                CancellationToken.None);
            await db.SaveChangesAsync();
            lastEvent = result.IntegrationEvent;
        }

        var manifest = await db.PackingManifests
            .AsNoTracking()
            .Include(entity => entity.Items)
            .SingleAsync(entity => entity.OrderId == order.Payload.OrderId);
        Assert.Equal(3, manifest.ReadyItemsCount);
        Assert.Equal(PackingManifestStatus.ReadyForPacking, manifest.Status);
        Assert.IsType<IntegrationEventEnvelope<OrderReadyForPacking>>(lastEvent);
    }

    [Fact]
    public async Task DuplicateItemPreparationCompleted_DoesNotIncreaseReadyCount()
    {
        await using var db = await CreateDbContextAsync();
        var order = CreateOrder(lineCount: 1);
        var line = order.Payload.Items[0];
        await PackingManifestActions.CreateManifestAsync(order, db, CancellationToken.None);
        await db.SaveChangesAsync();

        var completed = Envelope(new ItemPreparationCompleted(order.Payload.OrderId, line.OrderItemId, "HOT"));
        await PackingManifestActions.CompleteItemAsync(completed, db, CancellationToken.None);
        await db.SaveChangesAsync();
        var duplicate = await PackingManifestActions.CompleteItemAsync(completed, db, CancellationToken.None);
        await db.SaveChangesAsync();

        var manifest = await db.PackingManifests
            .AsNoTracking()
            .Include(entity => entity.Items)
            .SingleAsync(entity => entity.OrderId == order.Payload.OrderId);
        Assert.Equal(1, manifest.ReadyItemsCount);
        Assert.Null(duplicate.IntegrationEvent);
    }

    [Fact]
    public async Task ItemPreparationCompletedBeforeOrderAccepted_IsAppliedWhenManifestIsCreated()
    {
        await using var db = await CreateDbContextAsync();
        var order = CreateOrder(lineCount: 1);
        var line = order.Payload.Items[0];

        await PackingManifestActions.CompleteItemAsync(
            Envelope(new ItemPreparationCompleted(order.Payload.OrderId, line.OrderItemId, "COLD")),
            db,
            CancellationToken.None);
        await db.SaveChangesAsync();
        var created = await PackingManifestActions.CreateManifestAsync(order, db, CancellationToken.None);
        await db.SaveChangesAsync();

        var manifest = await db.PackingManifests
            .AsNoTracking()
            .Include(entity => entity.Items)
            .SingleAsync(entity => entity.OrderId == order.Payload.OrderId);
        Assert.Equal(1, manifest.ReadyItemsCount);
        Assert.Equal(PackingManifestStatus.ReadyForPacking, manifest.Status);
        Assert.IsType<IntegrationEventEnvelope<OrderReadyForPacking>>(created.IntegrationEvent);
        Assert.Equal(0, await db.PendingPreparedItems.CountAsync(item => item.OrderId == order.Payload.OrderId));
    }

    [Fact]
    public async Task ReadyManifest_CanBeIssuedAndProducesPickupEvent()
    {
        await using var db = await CreateDbContextAsync();
        var order = CreateOrder(lineCount: 1);
        var line = order.Payload.Items[0];
        await PackingManifestActions.CreateManifestAsync(order, db, CancellationToken.None);
        await db.SaveChangesAsync();
        await PackingManifestActions.CompleteItemAsync(
            Envelope(new ItemPreparationCompleted(order.Payload.OrderId, line.OrderItemId, "HOT")),
            db,
            CancellationToken.None);
        await db.SaveChangesAsync();
        var manifest = await db.PackingManifests.SingleAsync(entity => entity.OrderId == order.Payload.OrderId);

        var now = DateTimeOffset.UtcNow;
        var issued = manifest.MarkIssued(now);
        var integrationEvent = PackingEventFactory.OrderReadyForPickup(manifest, now);
        await db.SaveChangesAsync();

        Assert.True(issued);
        Assert.Equal(PackingManifestStatus.Issued, manifest.Status);
        Assert.Equal(order.Payload.OrderId, integrationEvent.Payload.OrderId);
        Assert.StartsWith("PU-", integrationEvent.Payload.PickupCode, StringComparison.Ordinal);
    }

    [Fact]
    public void OldWaitingManifest_IsMappedAsDelayed()
    {
        var manifest = PackingManifest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "test",
            DateTimeOffset.UtcNow.AddMinutes(-31));
        manifest.AddItem(ManifestItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Burger", 1));

        var response = PackingManifestResponse.FromManifest(
            manifest,
            DateTimeOffset.UtcNow,
            PackingOptions.DelayThreshold);

        Assert.True(response.IsDelayed);
        Assert.Equal("Delayed", response.Status);
    }

    private static IntegrationEventEnvelope<OrderAccepted> CreateOrder(int lineCount)
    {
        var lines = Enumerable.Range(0, lineCount)
            .Select(index => new OrderAcceptedLine(
                Guid.NewGuid(),
                Guid.NewGuid(),
                $"Item {index}",
                1))
            .ToArray();

        return Envelope(new OrderAccepted(Guid.NewGuid(), "test", lines));
    }
}
