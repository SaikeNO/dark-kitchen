using DarkKitchen.Contracts.Events;
using Microsoft.AspNetCore.SignalR;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Packing.Features.Features.Packing;

public static class ItemPreparationCompletedHandler
{
    public static async Task Handle(
        IntegrationEventEnvelope<ItemPreparationCompleted> envelope,
        IDbContextOutbox<PackingDbContext> outbox,
        IHubContext<PackingHub> hub,
        CancellationToken ct)
    {
        var result = await PackingManifestActions.CompleteItemAsync(envelope, outbox.DbContext, ct);
        await OrderAcceptedHandler.PersistPublishAndNotifyAsync(result, outbox, hub, ct);
    }
}
