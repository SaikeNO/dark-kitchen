using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace DarkKitchen.OrderManagement.Features.Features.Inventory;

public static class InventoryReservedHandler
{
    public static async Task Handle(
        IntegrationEventEnvelope<InventoryReserved> envelope,
        OrderManagementDbContext db,
        IMessageBus bus,
        CancellationToken ct)
    {
        var accepted = await AcceptAsync(envelope, db, ct);
        if (accepted is not null)
        {
            await bus.PublishAsync(accepted);
        }
    }

    public static async Task<IntegrationEventEnvelope<OrderAccepted>?> AcceptAsync(
        IntegrationEventEnvelope<InventoryReserved> envelope,
        OrderManagementDbContext db,
        CancellationToken ct)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(entity => entity.Items)
            .FirstOrDefaultAsync(entity => entity.Id == envelope.Payload.OrderId, ct);

        if (order is null || order.Status != OrderStatus.Placed)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var updatedRows = await db.Orders
            .Where(entity => entity.Id == envelope.Payload.OrderId && entity.Status == OrderStatus.Placed)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => entity.Status, OrderStatus.Accepted)
                .SetProperty(entity => entity.UpdatedAt, now), ct);

        if (updatedRows == 0)
        {
            return null;
        }

        db.OrderHistories.Add(OrderHistory.Create(
            order.Id,
            OrderStatus.Placed,
            OrderStatus.Accepted,
            envelope.CorrelationId,
            now,
            "Inventory reserved"));
        await db.SaveChangesAsync(ct);
        return OrderManagementEventFactory.OrderAccepted(order, envelope);
    }
}
