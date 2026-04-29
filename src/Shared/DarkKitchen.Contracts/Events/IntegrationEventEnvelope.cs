namespace DarkKitchen.Contracts.Events;

public sealed record IntegrationEventEnvelope<TPayload>(
    Guid EventId,
    string EventType,
    DateTimeOffset OccurredAt,
    Guid CorrelationId,
    Guid? CausationId,
    int SchemaVersion,
    string? BrandId,
    TPayload Payload);
