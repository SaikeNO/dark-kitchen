using DarkKitchen.Contracts.Events;

namespace DarkKitchen.OrderManagement.Features.Features.Progress;

public static class OrderCompletedHandler
{
    public static Task Handle(
        IntegrationEventEnvelope<OrderCompleted> envelope,
        OrderManagementDbContext db,
        CancellationToken ct)
    {
        return ItemPreparationStartedHandler.ApplyAsync(
            envelope.Payload.OrderId,
            envelope.CorrelationId,
            OrderStatus.Completed,
            db,
            ct);
    }
}
