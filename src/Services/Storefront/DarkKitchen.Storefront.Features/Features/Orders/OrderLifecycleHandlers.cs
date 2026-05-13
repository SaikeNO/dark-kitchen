using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Storefront.Features.Features.Orders;

public static class OrderLifecycleHandlers
{
    public static Task Handle(IntegrationEventEnvelope<OrderAccepted> envelope, StorefrontDbContext db, CancellationToken ct)
    {
        return ApplyAsync(envelope.Payload.OrderId, "Accepted", db, ct);
    }

    public static Task Handle(IntegrationEventEnvelope<InventoryReservationFailed> envelope, StorefrontDbContext db, CancellationToken ct)
    {
        return ApplyAsync(envelope.Payload.OrderId, "Rejected", db, ct, failureReason: envelope.Payload.ReasonCode);
    }

    public static Task Handle(IntegrationEventEnvelope<ItemPreparationStarted> envelope, StorefrontDbContext db, CancellationToken ct)
    {
        return ApplyAsync(envelope.Payload.OrderId, "Preparing", db, ct);
    }

    public static Task Handle(IntegrationEventEnvelope<OrderReadyForPacking> envelope, StorefrontDbContext db, CancellationToken ct)
    {
        return ApplyAsync(envelope.Payload.OrderId, "ReadyForPacking", db, ct);
    }

    public static Task Handle(IntegrationEventEnvelope<OrderReadyForPickup> envelope, StorefrontDbContext db, CancellationToken ct)
    {
        return ApplyAsync(envelope.Payload.OrderId, "ReadyForPickup", db, ct, pickupCode: envelope.Payload.PickupCode);
    }

    public static Task Handle(IntegrationEventEnvelope<OrderCompleted> envelope, StorefrontDbContext db, CancellationToken ct)
    {
        return ApplyAsync(envelope.Payload.OrderId, "Completed", db, ct);
    }

    private static async Task ApplyAsync(
        Guid orderId,
        string status,
        StorefrontDbContext db,
        CancellationToken ct,
        string? failureReason = null,
        string? pickupCode = null)
    {
        var order = await db.CustomerOrders.FirstOrDefaultAsync(order => order.OrderId == orderId, ct);
        if (order is null)
        {
            return;
        }

        order.ApplyStatus(status, DateTimeOffset.UtcNow, failureReason, pickupCode);
        await db.SaveChangesAsync(ct);
    }
}
