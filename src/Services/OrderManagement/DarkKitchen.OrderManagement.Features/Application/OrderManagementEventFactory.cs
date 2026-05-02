using DarkKitchen.Contracts.Events;

namespace DarkKitchen.OrderManagement.Features.Application;

public static class OrderManagementEventFactory
{
    public static IntegrationEventEnvelope<OrderPlaced> OrderPlaced(Order order)
    {
        return IntegrationEventEnvelope.Create(
            eventId: Guid.NewGuid(),
            occurredAt: DateTimeOffset.UtcNow,
            correlationId: order.CorrelationId,
            causationId: null,
            brandId: order.BrandId.ToString("D"),
            payload: new OrderPlaced(
                order.Id,
                order.SourceChannel,
                order.Items
                    .OrderBy(item => item.Id)
                    .Select(item => new OrderPlacedLine(item.Id, item.MenuItemId, item.Quantity))
                    .ToArray()));
    }

    public static IntegrationEventEnvelope<OrderAccepted> OrderAccepted(
        Order order,
        IntegrationEventEnvelope<InventoryReserved> source)
    {
        return IntegrationEventEnvelope.Create(
            eventId: Guid.NewGuid(),
            occurredAt: DateTimeOffset.UtcNow,
            correlationId: source.CorrelationId,
            causationId: source.EventId,
            brandId: order.BrandId.ToString("D"),
            payload: new OrderAccepted(
                order.Id,
                order.SourceChannel,
                order.Items
                    .OrderBy(item => item.Id)
                    .Select(item => new OrderAcceptedLine(item.Id, item.MenuItemId, item.Name, item.Quantity))
                    .ToArray()));
    }
}
