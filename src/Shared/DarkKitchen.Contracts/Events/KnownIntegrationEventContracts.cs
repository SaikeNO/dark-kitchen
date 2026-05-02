namespace DarkKitchen.Contracts.Events;

public sealed record IntegrationEventContract(
    Type PayloadType,
    Type EnvelopeType,
    string EventType,
    int SchemaVersion);

public static class KnownIntegrationEventContracts
{
    public static readonly IntegrationEventContract OrderPlaced = Create<OrderPlaced>(KnownIntegrationEventTypes.OrderPlaced);

    public static readonly IntegrationEventContract InventoryReserved =
        Create<InventoryReserved>(KnownIntegrationEventTypes.InventoryReserved);

    public static readonly IntegrationEventContract InventoryReservationFailed =
        Create<InventoryReservationFailed>(KnownIntegrationEventTypes.InventoryReservationFailed);

    public static readonly IntegrationEventContract OrderAccepted = Create<OrderAccepted>(KnownIntegrationEventTypes.OrderAccepted);

    public static readonly IntegrationEventContract ItemPreparationStarted =
        Create<ItemPreparationStarted>(KnownIntegrationEventTypes.ItemPreparationStarted);

    public static readonly IntegrationEventContract ItemPreparationCompleted =
        Create<ItemPreparationCompleted>(KnownIntegrationEventTypes.ItemPreparationCompleted);

    public static readonly IntegrationEventContract OrderReadyForPacking =
        Create<OrderReadyForPacking>(KnownIntegrationEventTypes.OrderReadyForPacking);

    public static readonly IntegrationEventContract OrderReadyForPickup =
        Create<OrderReadyForPickup>(KnownIntegrationEventTypes.OrderReadyForPickup);

    public static readonly IntegrationEventContract MenuItemChanged =
        Create<MenuItemChanged>(KnownIntegrationEventTypes.MenuItemChanged);

    public static readonly IntegrationEventContract ProductPriceChanged =
        Create<ProductPriceChanged>(KnownIntegrationEventTypes.ProductPriceChanged);

    public static readonly IntegrationEventContract BrandChanged =
        Create<BrandChanged>(KnownIntegrationEventTypes.BrandChanged);

    public static readonly IntegrationEventContract CategoryChanged =
        Create<CategoryChanged>(KnownIntegrationEventTypes.CategoryChanged);

    public static readonly IntegrationEventContract RecipeChanged =
        Create<RecipeChanged>(KnownIntegrationEventTypes.RecipeChanged);

    public static readonly IntegrationEventContract StationChanged =
        Create<StationChanged>(KnownIntegrationEventTypes.StationChanged);

    public static readonly IntegrationEventContract ProductStationRoutingChanged =
        Create<ProductStationRoutingChanged>(KnownIntegrationEventTypes.ProductStationRoutingChanged);

    public static IReadOnlyList<IntegrationEventContract> All { get; } =
    [
        OrderPlaced,
        InventoryReserved,
        InventoryReservationFailed,
        OrderAccepted,
        ItemPreparationStarted,
        ItemPreparationCompleted,
        OrderReadyForPacking,
        OrderReadyForPickup,
        MenuItemChanged,
        ProductPriceChanged,
        BrandChanged,
        CategoryChanged,
        RecipeChanged,
        StationChanged,
        ProductStationRoutingChanged
    ];

    public static string EventTypeFor<TPayload>()
    {
        return FindByPayloadType(typeof(TPayload)).EventType;
    }

    public static IntegrationEventContract FindByPayloadType(Type payloadType)
    {
        ArgumentNullException.ThrowIfNull(payloadType);

        return All.FirstOrDefault(contract => contract.PayloadType == payloadType)
            ?? throw new ArgumentOutOfRangeException(nameof(payloadType), payloadType, "Unknown integration event payload type.");
    }

    public static IntegrationEventContract FindByEventType(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("Event type is required.", nameof(eventType));
        }

        return All.FirstOrDefault(contract => string.Equals(contract.EventType, eventType, StringComparison.Ordinal))
            ?? throw new ArgumentOutOfRangeException(nameof(eventType), eventType, "Unknown integration event type.");
    }

    private static IntegrationEventContract Create<TPayload>(string eventType)
    {
        return new IntegrationEventContract(
            typeof(TPayload),
            typeof(IntegrationEventEnvelope<TPayload>),
            eventType,
            IntegrationEventJson.CurrentSchemaVersion);
    }
}
