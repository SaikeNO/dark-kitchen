using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.Features.Features.Progress;

public static class ItemPreparationStartedHandler
{
    public static Task Handle(
        IntegrationEventEnvelope<ItemPreparationStarted> envelope,
        OrderManagementDbContext db,
        CancellationToken ct)
    {
        return ApplyAsync(envelope.Payload.OrderId, envelope.CorrelationId, OrderStatus.Preparing, db, ct);
    }

    public static async Task ApplyAsync(
        Guid orderId,
        Guid correlationId,
        OrderStatus targetStatus,
        OrderManagementDbContext db,
        CancellationToken ct)
    {
        var currentStatus = await db.Orders
            .AsNoTracking()
            .Where(order => order.Id == orderId)
            .Select(order => (OrderStatus?)order.Status)
            .FirstOrDefaultAsync(ct);
        if (currentStatus is null)
        {
            return;
        }

        if (currentStatus.Value is OrderStatus.Completed or OrderStatus.Rejected or OrderStatus.Cancelled
            || currentStatus.Value >= targetStatus)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var updatedRows = await db.Orders
            .Where(order => order.Id == orderId && order.Status == currentStatus.Value)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(order => order.Status, targetStatus)
                .SetProperty(order => order.UpdatedAt, now), ct);

        if (updatedRows == 0)
        {
            return;
        }

        db.OrderHistories.Add(OrderHistory.Create(
            orderId,
            currentStatus.Value,
            targetStatus,
            correlationId,
            now,
            ReasonFor(targetStatus)));
        await db.SaveChangesAsync(ct);
    }

    private static string ReasonFor(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Preparing => "Preparation started",
            OrderStatus.ReadyForPacking => "Ready for packing",
            OrderStatus.ReadyForPickup => "Ready for pickup",
            _ => status.ToString()
        };
    }
}
