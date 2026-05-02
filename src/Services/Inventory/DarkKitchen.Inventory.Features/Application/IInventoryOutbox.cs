using DarkKitchen.Contracts.Events;

namespace DarkKitchen.Inventory.Features.Application;

public interface IInventoryOutbox
{
    InventoryDbContext DbContext { get; }
    Task PublishAsync<TPayload>(IntegrationEventEnvelope<TPayload> envelope, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task FlushAsync(CancellationToken ct);
}
