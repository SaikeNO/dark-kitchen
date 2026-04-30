using System.Text.Json;
using System.Text.Json.Nodes;
using DarkKitchen.Contracts.Events;

namespace DarkKitchen.ContractTests;

public sealed class EventEnvelopeContractTests
{
    private static readonly Guid EventId = Guid.Parse("3f9d2a21-45bc-4fd5-870c-b3b7c1e21301");
    private static readonly DateTimeOffset OccurredAt = DateTimeOffset.Parse("2026-04-29T18:00:00Z");
    private static readonly Guid CorrelationId = Guid.Parse("c7a10bb2-59fb-4b95-82e2-24b630a8b8c4");
    private static readonly Guid CausationId = Guid.Parse("d6bd26a2-b07a-4dd9-a27d-1de133c4d40e");
    private const string BrandId = "brand-burger-01";

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

    [Fact]
    public void KnownIntegrationEventContracts_MapEveryKnownEventToVersionedPayload()
    {
        Assert.Equal(KnownIntegrationEventTypes.All.Count, KnownIntegrationEventContracts.All.Count);

        Assert.All(KnownIntegrationEventContracts.All, contract =>
        {
            Assert.Contains(contract.EventType, KnownIntegrationEventTypes.All);
            Assert.Equal(IntegrationEventJson.CurrentSchemaVersion, contract.SchemaVersion);
            Assert.Equal(typeof(IntegrationEventEnvelope<>).MakeGenericType(contract.PayloadType), contract.EnvelopeType);
        });
    }

    [Fact]
    public void IntegrationEventTopology_HasStableExchangeAndQueueNames()
    {
        Assert.Equal("dark-kitchen.events.v1.order.placed", IntegrationEventTopology.ExchangeFor(KnownIntegrationEventTypes.OrderPlaced));
        Assert.Equal("dark-kitchen.inventory.v1", IntegrationEventTopology.QueueFor(DarkKitchenService.Inventory));
        Assert.Equal("dark-kitchen.order-management.v1", IntegrationEventTopology.QueueFor(DarkKitchenService.OrderManagement));
        Assert.Equal("dark-kitchen.kds.v1", IntegrationEventTopology.QueueFor(DarkKitchenService.Kds));
        Assert.Equal("dark-kitchen.packing.v1", IntegrationEventTopology.QueueFor(DarkKitchenService.Packing));
        Assert.Equal("dark-kitchen.storefront.v1", IntegrationEventTopology.QueueFor(DarkKitchenService.Storefront));
    }

    [Theory]
    [MemberData(nameof(EventSamples))]
    public void EventEnvelope_SerializesPublicContractAsCamelCase(
        IntegrationEventContract contract,
        object payload,
        string expectedJson)
    {
        var envelope = CreateEnvelope(contract, payload);
        var json = JsonSerializer.Serialize(envelope, contract.EnvelopeType, IntegrationEventJson.SerializerOptions);

        AssertJsonEqual(expectedJson, json);
        Assert.DoesNotContain("\"EventId\"", json);
        Assert.DoesNotContain("\"Payload\"", json);

        var restored = JsonSerializer.Deserialize(json, contract.EnvelopeType, IntegrationEventJson.SerializerOptions);

        Assert.NotNull(restored);
    }

    [Fact]
    public void IntegrationEventEnvelopeFactory_AssignsKnownEventTypeAndCurrentSchemaVersion()
    {
        var envelope = IntegrationEventEnvelope.Create(
            EventId,
            OccurredAt,
            CorrelationId,
            CausationId,
            BrandId,
            new OrderAccepted(Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad")));

        Assert.Equal(KnownIntegrationEventTypes.OrderAccepted, envelope.EventType);
        Assert.Equal(IntegrationEventJson.CurrentSchemaVersion, envelope.SchemaVersion);
    }

    [Fact]
    public void EventEnvelope_RejectsJsonWithoutRequiredCorrelationId()
    {
        const string json = """
        {
          "eventId": "3f9d2a21-45bc-4fd5-870c-b3b7c1e21301",
          "eventType": "order.accepted",
          "occurredAt": "2026-04-29T18:00:00+00:00",
          "causationId": null,
          "schemaVersion": 1,
          "brandId": "brand-burger-01",
          "payload": {
            "orderId": "0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"
          }
        }
        """;

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<IntegrationEventEnvelope<OrderAccepted>>(json, IntegrationEventJson.SerializerOptions));
    }

    [Fact]
    public void EventEnvelope_RejectsEmptyCorrelationId()
    {
        Assert.Throws<ArgumentException>(() => new IntegrationEventEnvelope<OrderAccepted>(
            eventId: EventId,
            eventType: KnownIntegrationEventTypes.OrderAccepted,
            occurredAt: OccurredAt,
            correlationId: Guid.Empty,
            causationId: null,
            schemaVersion: IntegrationEventJson.CurrentSchemaVersion,
            brandId: BrandId,
            payload: new OrderAccepted(Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"))));
    }

    public static TheoryData<IntegrationEventContract, object, string> EventSamples => new()
    {
        {
            KnownIntegrationEventContracts.OrderPlaced,
            new OrderPlaced(
                Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"),
                "storefront",
                [new OrderPlacedLine(Guid.Parse("11969867-31f0-4f9b-a12f-3de093659ec2"), Guid.Parse("887ff5ea-620b-4574-aeb9-9c97b3be4216"), 2)]),
            """
            {
              "eventId": "3f9d2a21-45bc-4fd5-870c-b3b7c1e21301",
              "eventType": "order.placed",
              "occurredAt": "2026-04-29T18:00:00+00:00",
              "correlationId": "c7a10bb2-59fb-4b95-82e2-24b630a8b8c4",
              "causationId": "d6bd26a2-b07a-4dd9-a27d-1de133c4d40e",
              "schemaVersion": 1,
              "brandId": "brand-burger-01",
              "payload": {
                "orderId": "0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad",
                "sourceChannel": "storefront",
                "items": [
                  {
                    "orderItemId": "11969867-31f0-4f9b-a12f-3de093659ec2",
                    "menuItemId": "887ff5ea-620b-4574-aeb9-9c97b3be4216",
                    "quantity": 2
                  }
                ]
              }
            }
            """
        },
        {
            KnownIntegrationEventContracts.InventoryReserved,
            new InventoryReserved(Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"), Guid.Parse("ebde5dcc-06c0-4e7f-a1d0-f3c83cbca4af")),
            """
            {
              "eventId": "3f9d2a21-45bc-4fd5-870c-b3b7c1e21301",
              "eventType": "inventory.reserved",
              "occurredAt": "2026-04-29T18:00:00+00:00",
              "correlationId": "c7a10bb2-59fb-4b95-82e2-24b630a8b8c4",
              "causationId": "d6bd26a2-b07a-4dd9-a27d-1de133c4d40e",
              "schemaVersion": 1,
              "brandId": "brand-burger-01",
              "payload": {
                "orderId": "0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad",
                "reservationId": "ebde5dcc-06c0-4e7f-a1d0-f3c83cbca4af"
              }
            }
            """
        },
        {
            KnownIntegrationEventContracts.InventoryReservationFailed,
            new InventoryReservationFailed(Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"), "ingredient_unavailable"),
            """
            {
              "eventId": "3f9d2a21-45bc-4fd5-870c-b3b7c1e21301",
              "eventType": "inventory.reservation_failed",
              "occurredAt": "2026-04-29T18:00:00+00:00",
              "correlationId": "c7a10bb2-59fb-4b95-82e2-24b630a8b8c4",
              "causationId": "d6bd26a2-b07a-4dd9-a27d-1de133c4d40e",
              "schemaVersion": 1,
              "brandId": "brand-burger-01",
              "payload": {
                "orderId": "0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad",
                "reasonCode": "ingredient_unavailable"
              }
            }
            """
        },
        {
            KnownIntegrationEventContracts.OrderAccepted,
            new OrderAccepted(Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad")),
            """
            {
              "eventId": "3f9d2a21-45bc-4fd5-870c-b3b7c1e21301",
              "eventType": "order.accepted",
              "occurredAt": "2026-04-29T18:00:00+00:00",
              "correlationId": "c7a10bb2-59fb-4b95-82e2-24b630a8b8c4",
              "causationId": "d6bd26a2-b07a-4dd9-a27d-1de133c4d40e",
              "schemaVersion": 1,
              "brandId": "brand-burger-01",
              "payload": {
                "orderId": "0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"
              }
            }
            """
        },
        {
            KnownIntegrationEventContracts.ItemPreparationStarted,
            new ItemPreparationStarted(Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"), Guid.Parse("11969867-31f0-4f9b-a12f-3de093659ec2"), "grill"),
            """
            {
              "eventId": "3f9d2a21-45bc-4fd5-870c-b3b7c1e21301",
              "eventType": "item.preparation_started",
              "occurredAt": "2026-04-29T18:00:00+00:00",
              "correlationId": "c7a10bb2-59fb-4b95-82e2-24b630a8b8c4",
              "causationId": "d6bd26a2-b07a-4dd9-a27d-1de133c4d40e",
              "schemaVersion": 1,
              "brandId": "brand-burger-01",
              "payload": {
                "orderId": "0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad",
                "orderItemId": "11969867-31f0-4f9b-a12f-3de093659ec2",
                "stationCode": "grill"
              }
            }
            """
        },
        {
            KnownIntegrationEventContracts.ItemPreparationCompleted,
            new ItemPreparationCompleted(Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"), Guid.Parse("11969867-31f0-4f9b-a12f-3de093659ec2"), "grill"),
            """
            {
              "eventId": "3f9d2a21-45bc-4fd5-870c-b3b7c1e21301",
              "eventType": "item.preparation_completed",
              "occurredAt": "2026-04-29T18:00:00+00:00",
              "correlationId": "c7a10bb2-59fb-4b95-82e2-24b630a8b8c4",
              "causationId": "d6bd26a2-b07a-4dd9-a27d-1de133c4d40e",
              "schemaVersion": 1,
              "brandId": "brand-burger-01",
              "payload": {
                "orderId": "0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad",
                "orderItemId": "11969867-31f0-4f9b-a12f-3de093659ec2",
                "stationCode": "grill"
              }
            }
            """
        },
        {
            KnownIntegrationEventContracts.OrderReadyForPacking,
            new OrderReadyForPacking(Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad")),
            """
            {
              "eventId": "3f9d2a21-45bc-4fd5-870c-b3b7c1e21301",
              "eventType": "order.ready_for_packing",
              "occurredAt": "2026-04-29T18:00:00+00:00",
              "correlationId": "c7a10bb2-59fb-4b95-82e2-24b630a8b8c4",
              "causationId": "d6bd26a2-b07a-4dd9-a27d-1de133c4d40e",
              "schemaVersion": 1,
              "brandId": "brand-burger-01",
              "payload": {
                "orderId": "0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"
              }
            }
            """
        },
        {
            KnownIntegrationEventContracts.OrderReadyForPickup,
            new OrderReadyForPickup(Guid.Parse("0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad"), "A-42"),
            """
            {
              "eventId": "3f9d2a21-45bc-4fd5-870c-b3b7c1e21301",
              "eventType": "order.ready_for_pickup",
              "occurredAt": "2026-04-29T18:00:00+00:00",
              "correlationId": "c7a10bb2-59fb-4b95-82e2-24b630a8b8c4",
              "causationId": "d6bd26a2-b07a-4dd9-a27d-1de133c4d40e",
              "schemaVersion": 1,
              "brandId": "brand-burger-01",
              "payload": {
                "orderId": "0ab0f38c-d6b4-4b26-aa05-5ed6ad8515ad",
                "pickupCode": "A-42"
              }
            }
            """
        }
    };

    private static object CreateEnvelope(IntegrationEventContract contract, object payload)
    {
        return Activator.CreateInstance(
            contract.EnvelopeType,
            EventId,
            contract.EventType,
            OccurredAt,
            CorrelationId,
            CausationId,
            contract.SchemaVersion,
            BrandId,
            payload)!;
    }

    private static void AssertJsonEqual(string expectedJson, string actualJson)
    {
        var expected = JsonNode.Parse(expectedJson);
        var actual = JsonNode.Parse(actualJson);

        Assert.True(JsonNode.DeepEquals(expected, actual), actualJson);
    }
}
