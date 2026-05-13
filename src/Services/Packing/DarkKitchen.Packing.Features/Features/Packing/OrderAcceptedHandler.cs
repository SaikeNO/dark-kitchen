using DarkKitchen.Contracts.Events;
using Microsoft.AspNetCore.SignalR;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Packing.Features.Features.Packing;

public static class OrderAcceptedHandler
{
    public static async Task Handle(
        IntegrationEventEnvelope<OrderAccepted> envelope,
        IDbContextOutbox<PackingDbContext> outbox,
        IHubContext<PackingHub> hub,
        CancellationToken ct)
    {
        var result = await PackingManifestActions.CreateManifestAsync(envelope, outbox.DbContext, ct);
        await PersistPublishAndNotifyAsync(result, outbox, hub, ct);
    }

    internal static async Task PersistPublishAndNotifyAsync(
        PackingActionResult result,
        IDbContextOutbox<PackingDbContext> outbox,
        IHubContext<PackingHub> hub,
        CancellationToken ct)
    {
        await PublishAsync(result.IntegrationEvent, outbox);
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        if (result.Manifest is not null)
        {
            await PackingManifestNotifier.NotifyManifestChangedAsync(hub, result.Manifest, ct);
        }
    }

    internal static async Task PublishAsync(object? integrationEvent, IDbContextOutbox<PackingDbContext> outbox)
    {
        switch (integrationEvent)
        {
            case IntegrationEventEnvelope<OrderReadyForPacking> readyForPacking:
                await outbox.PublishAsync(readyForPacking);
                break;
            case IntegrationEventEnvelope<OrderReadyForPickup> readyForPickup:
                await outbox.PublishAsync(readyForPickup);
                break;
            case null:
                break;
            default:
                throw new InvalidOperationException($"Unsupported Packing integration event: {integrationEvent.GetType()}.");
        }
    }
}
