using System.Text.Json.Serialization;

namespace DarkKitchen.Contracts.Events;

public sealed record IntegrationEventEnvelope<TPayload>
{
    [JsonConstructor]
    public IntegrationEventEnvelope(
        Guid eventId,
        string eventType,
        DateTimeOffset occurredAt,
        Guid correlationId,
        Guid? causationId,
        int schemaVersion,
        string brandId,
        TPayload payload)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("EventId is required.", nameof(eventId));
        }

        if (occurredAt == default)
        {
            throw new ArgumentException("OccurredAt is required.", nameof(occurredAt));
        }

        if (correlationId == Guid.Empty)
        {
            throw new ArgumentException("CorrelationId is required.", nameof(correlationId));
        }

        if (schemaVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(schemaVersion), schemaVersion, "SchemaVersion must be positive.");
        }

        EventId = eventId;
        EventType = RequireNonWhiteSpace(eventType, nameof(eventType));
        OccurredAt = occurredAt;
        CorrelationId = correlationId;
        CausationId = causationId;
        SchemaVersion = schemaVersion;
        BrandId = RequireNonWhiteSpace(brandId, nameof(brandId));
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
    }

    [JsonRequired]
    public Guid EventId { get; init; }

    [JsonRequired]
    public string EventType { get; init; }

    [JsonRequired]
    public DateTimeOffset OccurredAt { get; init; }

    [JsonRequired]
    public Guid CorrelationId { get; init; }

    [JsonRequired]
    public Guid? CausationId { get; init; }

    [JsonRequired]
    public int SchemaVersion { get; init; }

    [JsonRequired]
    public string BrandId { get; init; }

    [JsonRequired]
    public TPayload Payload { get; init; }

    private static string RequireNonWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value;
    }
}

public static class IntegrationEventEnvelope
{
    public static IntegrationEventEnvelope<TPayload> Create<TPayload>(
        Guid eventId,
        DateTimeOffset occurredAt,
        Guid correlationId,
        Guid? causationId,
        string brandId,
        TPayload payload,
        int schemaVersion = IntegrationEventJson.CurrentSchemaVersion)
    {
        return new IntegrationEventEnvelope<TPayload>(
            eventId,
            KnownIntegrationEventContracts.EventTypeFor<TPayload>(),
            occurredAt,
            correlationId,
            causationId,
            schemaVersion,
            brandId,
            payload);
    }
}
