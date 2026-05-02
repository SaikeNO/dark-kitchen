using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.Features.Features.Inventory;

public static class InventoryReservationFailedHandler
{
    public static Task Handle(
        IntegrationEventEnvelope<InventoryReservationFailed> envelope,
        OrderManagementDbContext db,
        CancellationToken ct)
    {
        return RejectAsync(envelope, db, ct);
    }

    public static async Task RejectAsync(
        IntegrationEventEnvelope<InventoryReservationFailed> envelope,
        OrderManagementDbContext db,
        CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var updatedRows = await db.Orders
            .Where(entity => entity.Id == envelope.Payload.OrderId && entity.Status == OrderStatus.Placed)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => entity.Status, OrderStatus.Rejected)
                .SetProperty(entity => entity.UpdatedAt, now), ct);

        if (updatedRows == 0)
        {
            return;
        }

        db.OrderHistories.Add(OrderHistory.Create(
            envelope.Payload.OrderId,
            OrderStatus.Placed,
            OrderStatus.Rejected,
            envelope.CorrelationId,
            now,
            envelope.Payload.ReasonCode));
        await db.SaveChangesAsync(ct);
    }
}
