using System.Text.Json;
using DarkKitchen.Contracts.Events;

namespace DarkKitchen.ContractTests;

public sealed class EventEnvelopeContractTests
{
    [Fact]
    public void KnownIntegrationEventTypes_AreStableAndUnique()
    {
        Assert.Equal(8, KnownIntegrationEventTypes.All.Count);
        Assert.Equal(KnownIntegrationEventTypes.All.Count, KnownIntegrationEventTypes.All.Distinct(StringComparer.Ordinal).Count());
        Assert.All(KnownIntegrationEventTypes.All, eventType =>
        {
            Assert.NotEmpty(eventType);
            Assert.DoesNotContain(' ', eventType);
            Assert.Equal(eventType.ToLowerInvariant(), eventType);
        });
    }

    [Theory]
    [MemberData(nameof(EventSamples))]
    public void EventEnvelope_SerializesPublicContractAsCamelCase<TPayload>(
        string eventType,
        TPayload payload)
    {
        var envelope = new IntegrationEventEnvelope<TPayload>(
            EventId: Guid.Parse("3f9d2a21-45bc-4fd5-870c-b3b7c1e21301"),
            EventType: eventType,
            OccurredAt: DateTimeOffset.Parse("2026-04-29T18:00:00Z"),
            CorrelationId: Guid.Parse("c7a10bb2-59fb-4b95-82e2-24b630a8b8c4"),
            CausationId: Guid.Parse("d6bd26a2-b07a-4dd9-a27d-1de133c4d40e"),
            SchemaVersion: IntegrationEventJson.CurrentSchemaVersion,
            BrandId: "brand-burger-01",
            Payload: payload);

        var json = JsonSerializer.Serialize(envelope, IntegrationEventJson.SerializerOptions);

        Assert.Contains("\"eventId\"", json);
        Assert.Contains("\"eventType\"", json);
        Assert.Contains("\"occurredAt\"", json);
        Assert.Contains("\"correlationId\"", json);
        Assert.Contains("\"causationId\"", json);
        Assert.Contains("\"schemaVersion\"", json);
        Assert.Contains("\"brandId\"", json);
        Assert.Contains("\"payload\"", json);
        Assert.DoesNotContain("\"EventId\"", json);
        Assert.DoesNotContain("\"Payload\"", json);

        var restored = JsonSerializer.Deserialize<IntegrationEventEnvelope<TPayload>>(
            json,
            IntegrationEventJson.SerializerOptions);

        Assert.NotNull(restored);
        Assert.Equal(envelope.EventId, restored.EventId);
        Assert.Equal(envelope.EventType, restored.EventType);
        Assert.Equal(envelope.CorrelationId, restored.CorrelationId);
        Assert.Equal(envelope.SchemaVersion, restored.SchemaVersion);
        Assert.Equal(envelope.BrandId, restored.BrandId);
    }

    public static TheoryData<string, object> EventSamples => new()
    {
        {
            KnownIntegrationEventTypes.OrderPlaced,
            new OrderPlaced(
                Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"),
                "storefront",
                [new OrderPlacedLine(Guid.Parse("11969867-31f0-4f9b-a12f-3de093659ec2"), Guid.Parse("887ff5ea-620b-4574-aeb9-9c97b3be4216"), 2)])
        },
        {
            KnownIntegrationEventTypes.InventoryReserved,
            new InventoryReserved(Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"), Guid.Parse("ebde5dcc-06c0-4e7f-a1d0-f3c83cbca4af"))
        },
        {
            KnownIntegrationEventTypes.InventoryReservationFailed,
            new InventoryReservationFailed(Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"), "ingredient_unavailable")
        },
        {
            KnownIntegrationEventTypes.OrderAccepted,
            new OrderAccepted(Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"))
        },
        {
            KnownIntegrationEventTypes.ItemPreparationStarted,
            new ItemPreparationStarted(Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"), Guid.Parse("11969867-31f0-4f9b-a12f-3de093659ec2"), "grill")
        },
        {
            KnownIntegrationEventTypes.ItemPreparationCompleted,
            new ItemPreparationCompleted(Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"), Guid.Parse("11969867-31f0-4f9b-a12f-3de093659ec2"), "grill")
        },
        {
            KnownIntegrationEventTypes.OrderReadyForPacking,
            new OrderReadyForPacking(Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"))
        },
        {
            KnownIntegrationEventTypes.OrderReadyForPickup,
            new OrderReadyForPickup(Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"), "A-42")
        }
    };
}
