using DarkKitchen.Contracts.Events;

namespace DarkKitchen.Inventory.Features.Application;

public static class InventoryEventFactory
{
    public static IntegrationEventEnvelope<InventoryReserved> Reserved(
        IntegrationEventEnvelope<OrderPlaced> source,
        Guid reservationId)
    {
        return Create(source, new InventoryReserved(source.Payload.OrderId, reservationId));
    }

    public static IntegrationEventEnvelope<InventoryReservationFailed> Failed(
        IntegrationEventEnvelope<OrderPlaced> source,
        string reasonCode)
    {
        return Create(source, new InventoryReservationFailed(source.Payload.OrderId, reasonCode));
    }

    private static IntegrationEventEnvelope<TPayload> Create<TPayload>(
        IntegrationEventEnvelope<OrderPlaced> source,
        TPayload payload)
    {
        return IntegrationEventEnvelope.Create(
            eventId: Guid.NewGuid(),
            occurredAt: DateTimeOffset.UtcNow,
            correlationId: source.CorrelationId,
            causationId: source.EventId,
            brandId: source.BrandId,
            payload: payload);
    }
}
