using DarkKitchen.Contracts.Events;

namespace DarkKitchen.Inventory.Features.Handlers;

public static class OrderPlacedHandler
{
    public static async Task Handle(
        IntegrationEventEnvelope<OrderPlaced> envelope,
        InventoryReservationService service,
        CancellationToken ct)
    {
        await service.HandleOrderPlacedAsync(envelope, ct);
    }
}
