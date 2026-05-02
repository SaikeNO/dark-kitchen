using DarkKitchen.Contracts.Events;

namespace DarkKitchen.OrderManagement.Features.Features.Progress;

public static class OrderReadyForPickupHandler
{
    public static Task Handle(
        IntegrationEventEnvelope<OrderReadyForPickup> envelope,
        OrderManagementDbContext db,
        CancellationToken ct)
    {
        return ItemPreparationStartedHandler.ApplyAsync(
            envelope.Payload.OrderId,
            envelope.CorrelationId,
            OrderStatus.ReadyForPickup,
            db,
            ct);
    }
}
