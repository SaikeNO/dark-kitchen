using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Inventory.Features.Features.Orders;

public static class ReservationLifecycleHandlers
{
    public static Task Handle(
        IntegrationEventEnvelope<OrderCancelled> envelope,
        InventoryDbContext db,
        CancellationToken ct)
    {
        return ApplyAsync(envelope.Payload.OrderId, db, ReservationAction.Release, ct);
    }

    public static Task Handle(
        IntegrationEventEnvelope<OrderCompleted> envelope,
        InventoryDbContext db,
        CancellationToken ct)
    {
        return ApplyAsync(envelope.Payload.OrderId, db, ReservationAction.Consume, ct);
    }

    public static async Task ApplyAsync(
        Guid orderId,
        InventoryDbContext db,
        ReservationAction action,
        CancellationToken ct)
    {
        var reservation = await db.StockReservations
            .Include(reservation => reservation.Lines)
            .FirstOrDefaultAsync(reservation => reservation.OrderId == orderId, ct);
        if (reservation is null || reservation.Status != StockReservationStatus.Reserved)
        {
            return;
        }

        var itemIds = reservation.Lines.Select(line => line.WarehouseItemId).ToArray();
        var items = await db.WarehouseItems
            .Where(item => itemIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, ct);
        var now = DateTimeOffset.UtcNow;

        foreach (var line in reservation.Lines.OrderBy(line => line.WarehouseItemId))
        {
            if (!items.TryGetValue(line.WarehouseItemId, out var item))
            {
                continue;
            }

            await db.Entry(item).ReloadAsync(ct);
            if (action == ReservationAction.Release)
            {
                item.Release(line.Quantity, now);
                db.InventoryLogs.Add(InventoryLog.Create(
                    item.Id,
                    InventoryLogChangeType.ReservationRelease,
                    line.Quantity,
                    item.OnHandQuantity,
                    item.ReservedQuantity,
                    now,
                    orderId,
                    reservation.Id,
                    "Order reservation released"));
            }
            else
            {
                item.ConsumeReserved(line.Quantity, now);
                db.InventoryLogs.Add(InventoryLog.Create(
                    item.Id,
                    InventoryLogChangeType.Consumption,
                    -line.Quantity,
                    item.OnHandQuantity,
                    item.ReservedQuantity,
                    now,
                    orderId,
                    reservation.Id,
                    "Order reservation consumed"));
            }
        }

        if (action == ReservationAction.Release)
        {
            reservation.Release();
        }
        else
        {
            reservation.Consume();
        }

        await db.SaveChangesAsync(ct);
    }
}

public enum ReservationAction
{
    Release,
    Consume
}
