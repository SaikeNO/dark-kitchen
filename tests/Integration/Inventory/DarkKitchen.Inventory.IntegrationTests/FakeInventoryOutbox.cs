using DarkKitchen.Contracts.Events;
using DarkKitchen.Inventory.Features.Application;

namespace DarkKitchen.Inventory.IntegrationTests;

public sealed class FakeInventoryOutbox(InventoryDbContext dbContext) : IInventoryOutbox
{
    public List<object> Published { get; } = [];

    public InventoryDbContext DbContext => dbContext;

    public Task PublishAsync<TPayload>(IntegrationEventEnvelope<TPayload> envelope, CancellationToken ct)
    {
        Published.Add(envelope);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await dbContext.SaveChangesAsync(ct);
    }

    public Task FlushAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
