using DarkKitchen.Contracts.Events;

namespace DarkKitchen.Packing.Features.Application;

public static class PackingEventFactory
{
    public static IntegrationEventEnvelope<OrderReadyForPacking> OrderReadyForPacking(
        PackingManifest manifest,
        DateTimeOffset now)
    {
        return CreateEnvelope(
            manifest,
            new OrderReadyForPacking(manifest.OrderId),
            now);
    }

    public static IntegrationEventEnvelope<OrderReadyForPickup> OrderReadyForPickup(
        PackingManifest manifest,
        DateTimeOffset now)
    {
        return CreateEnvelope(
            manifest,
            new OrderReadyForPickup(manifest.OrderId, PickupCodeFor(manifest.OrderId)),
            now);
    }

    public static string PickupCodeFor(Guid orderId)
    {
        return $"PU-{orderId:N}"[..11].ToUpperInvariant();
    }

    private static IntegrationEventEnvelope<TPayload> CreateEnvelope<TPayload>(
        PackingManifest manifest,
        TPayload payload,
        DateTimeOffset now)
    {
        return IntegrationEventEnvelope.Create(
            eventId: Guid.NewGuid(),
            occurredAt: now,
            correlationId: manifest.CorrelationId,
            causationId: null,
            brandId: manifest.BrandId.ToString("D"),
            payload: payload);
    }
}
