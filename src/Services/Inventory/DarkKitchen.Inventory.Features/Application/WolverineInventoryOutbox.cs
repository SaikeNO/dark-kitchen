using DarkKitchen.Contracts.Events;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Inventory.Features.Application;

public sealed class WolverineInventoryOutbox(IDbContextOutbox<InventoryDbContext> outbox) : IInventoryOutbox
{
    public InventoryDbContext DbContext => outbox.DbContext;

    public async Task PublishAsync<TPayload>(IntegrationEventEnvelope<TPayload> envelope, CancellationToken ct)
    {
        await outbox.PublishAsync(envelope);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await outbox.DbContext.SaveChangesAsync(ct);
    }

    public async Task FlushAsync(CancellationToken ct)
    {
        await outbox.FlushOutgoingMessagesAsync();
    }
}
