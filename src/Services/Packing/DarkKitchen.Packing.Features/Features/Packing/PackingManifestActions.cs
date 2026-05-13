using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Packing.Features.Features.Packing;

public enum PackingActionError
{
    None,
    NotFound,
    Conflict
}

public sealed record PackingActionResult(
    PackingActionError Error,
    PackingManifest? Manifest,
    object? IntegrationEvent,
    object? SecondaryIntegrationEvent,
    string? ErrorMessage);

public static class PackingManifestActions
{
    public static async Task<PackingActionResult> CreateManifestAsync(
        IntegrationEventEnvelope<OrderAccepted> envelope,
        PackingDbContext db,
        CancellationToken ct)
    {
        var order = envelope.Payload;
        var existing = await LoadManifestByOrderIdAsync(order.OrderId, db, ct);
        if (existing is not null)
        {
            return new PackingActionResult(PackingActionError.None, existing, null, null, null);
        }

        var brandId = Guid.TryParse(envelope.BrandId, out var parsedBrandId)
            ? parsedBrandId
            : Guid.Empty;
        var now = DateTimeOffset.UtcNow;
        var manifest = PackingManifest.Create(
            order.OrderId,
            brandId,
            envelope.CorrelationId,
            order.SourceChannel,
            now);

        foreach (var line in order.Items.OrderBy(item => item.OrderItemId))
        {
            manifest.AddItem(ManifestItem.Create(
                line.OrderItemId,
                line.MenuItemId,
                line.Name,
                line.Quantity));
        }

        var pendingItems = await db.PendingPreparedItems
            .Where(item => item.OrderId == order.OrderId)
            .ToArrayAsync(ct);
        foreach (var pendingItem in pendingItems)
        {
            manifest.MarkItemReady(pendingItem.OrderItemId, pendingItem.CompletedAt);
        }

        db.PackingManifests.Add(manifest);
        db.PendingPreparedItems.RemoveRange(pendingItems);

        var integrationEvent = manifest.Status == PackingManifestStatus.ReadyForPacking
            ? PackingEventFactory.OrderReadyForPacking(manifest, now)
            : null;

        return new PackingActionResult(PackingActionError.None, manifest, integrationEvent, null, null);
    }

    public static async Task<PackingActionResult> CompleteItemAsync(
        IntegrationEventEnvelope<ItemPreparationCompleted> envelope,
        PackingDbContext db,
        CancellationToken ct)
    {
        var completed = envelope.Payload;
        var now = DateTimeOffset.UtcNow;
        var manifest = await LoadManifestByOrderIdAsync(completed.OrderId, db, ct);
        if (manifest is null)
        {
            var pendingExists = await db.PendingPreparedItems
                .AnyAsync(item => item.OrderItemId == completed.OrderItemId, ct);
            if (!pendingExists)
            {
                db.PendingPreparedItems.Add(PendingPreparedItem.Create(
                    completed.OrderId,
                    completed.OrderItemId,
                    completed.StationCode,
                    envelope.CorrelationId,
                    envelope.BrandId,
                    envelope.OccurredAt,
                    now));
            }

            return new PackingActionResult(PackingActionError.None, null, null, null, null);
        }

        var wasReadyForPacking = manifest.Status == PackingManifestStatus.ReadyForPacking;
        manifest.MarkItemReady(completed.OrderItemId, envelope.OccurredAt);
        var integrationEvent = !wasReadyForPacking && manifest.Status == PackingManifestStatus.ReadyForPacking
            ? PackingEventFactory.OrderReadyForPacking(manifest, now)
            : null;

            return new PackingActionResult(PackingActionError.None, manifest, integrationEvent, null, null);
    }

    public static async Task<PackingActionResult> IssueAsync(
        Guid manifestId,
        string? pickupCode,
        IDbContextOutbox<PackingDbContext> outbox,
        CancellationToken ct)
    {
        var manifest = await LoadManifestByIdAsync(manifestId, outbox.DbContext, ct);
        if (manifest is null)
        {
            return new PackingActionResult(PackingActionError.NotFound, null, null, null, null);
        }

        var expectedPickupCode = PackingEventFactory.PickupCodeFor(manifest.OrderId);
        if (!string.Equals(pickupCode?.Trim(), expectedPickupCode, StringComparison.OrdinalIgnoreCase))
        {
            return new PackingActionResult(PackingActionError.Conflict, manifest, null, null, "Pickup code is invalid.");
        }

        var now = DateTimeOffset.UtcNow;
        try
        {
            var changed = manifest.MarkIssued(now);
            return new PackingActionResult(
                PackingActionError.None,
                manifest,
                changed ? PackingEventFactory.OrderReadyForPickup(manifest, now) : null,
                changed ? PackingEventFactory.OrderCompleted(manifest, now) : null,
                null);
        }
        catch (InvalidOperationException ex)
        {
            return new PackingActionResult(PackingActionError.Conflict, manifest, null, null, ex.Message);
        }
    }

    public static Task<PackingManifest?> LoadManifestByOrderIdAsync(
        Guid orderId,
        PackingDbContext db,
        CancellationToken ct)
    {
        return db.PackingManifests
            .Include(manifest => manifest.Items)
            .FirstOrDefaultAsync(manifest => manifest.OrderId == orderId, ct);
    }

    public static Task<PackingManifest?> LoadManifestByIdAsync(
        Guid manifestId,
        PackingDbContext db,
        CancellationToken ct)
    {
        return db.PackingManifests
            .Include(manifest => manifest.Items)
            .FirstOrDefaultAsync(manifest => manifest.Id == manifestId, ct);
    }
}
