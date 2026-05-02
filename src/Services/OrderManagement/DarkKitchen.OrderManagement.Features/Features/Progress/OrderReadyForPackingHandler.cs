using DarkKitchen.Contracts.Events;

namespace DarkKitchen.OrderManagement.Features.Features.Progress;

public static class OrderReadyForPackingHandler
{
    public static Task Handle(
        IntegrationEventEnvelope<OrderReadyForPacking> envelope,
        OrderManagementDbContext db,
        CancellationToken ct)
    {
        return ItemPreparationStartedHandler.ApplyAsync(
            envelope.Payload.OrderId,
            envelope.CorrelationId,
            OrderStatus.ReadyForPacking,
            db,
            ct);
    }
}
