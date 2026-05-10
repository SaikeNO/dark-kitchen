using DarkKitchen.Contracts.Events;

namespace DarkKitchen.Kds.Features.Application;

public static class KdsEventFactory
{
    public static IntegrationEventEnvelope<ItemPreparationStarted> ItemPreparationStarted(
        KitchenTask task,
        DateTimeOffset now)
    {
        return CreateEnvelope(
            task,
            new ItemPreparationStarted(
                task.Ticket!.OrderId,
                task.OrderItemId,
                RequiredStationCode(task)),
            now);
    }

    public static IntegrationEventEnvelope<ItemPreparationCompleted> ItemPreparationCompleted(
        KitchenTask task,
        DateTimeOffset now)
    {
        return CreateEnvelope(
            task,
            new ItemPreparationCompleted(
                task.Ticket!.OrderId,
                task.OrderItemId,
                RequiredStationCode(task)),
            now);
    }

    private static IntegrationEventEnvelope<TPayload> CreateEnvelope<TPayload>(
        KitchenTask task,
        TPayload payload,
        DateTimeOffset now)
    {
        var ticket = task.Ticket ?? throw new InvalidOperationException("Ticket must be loaded for KDS events.");

        return IntegrationEventEnvelope.Create(
            eventId: Guid.NewGuid(),
            occurredAt: now,
            correlationId: ticket.CorrelationId,
            causationId: null,
            brandId: ticket.BrandId.ToString("D"),
            payload: payload);
    }

    private static string RequiredStationCode(KitchenTask task)
    {
        return string.IsNullOrWhiteSpace(task.StationCode)
            ? throw new InvalidOperationException("Station code is required for KDS events.")
            : task.StationCode;
    }
}
