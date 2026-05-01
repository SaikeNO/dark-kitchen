namespace DarkKitchen.Contracts.Events;

public enum DarkKitchenService
{
    Catalog,
    Inventory,
    Kds,
    OrderManagement,
    Packing,
    Storefront
}

public sealed record IntegrationEventSubscription(
    DarkKitchenService Service,
    string QueueName,
    IReadOnlySet<string> EventTypes);

public static class IntegrationEventTopology
{
    public const string ExchangePrefix = "dark-kitchen.events.v1";

    public const string CatalogQueue = "dark-kitchen.catalog.v1";
    public const string InventoryQueue = "dark-kitchen.inventory.v1";
    public const string KdsQueue = "dark-kitchen.kds.v1";
    public const string OrderManagementQueue = "dark-kitchen.order-management.v1";
    public const string PackingQueue = "dark-kitchen.packing.v1";
    public const string StorefrontQueue = "dark-kitchen.storefront.v1";

    private static readonly IReadOnlyDictionary<DarkKitchenService, IntegrationEventSubscription> SubscriptionsByService =
        new Dictionary<DarkKitchenService, IntegrationEventSubscription>
        {
            [DarkKitchenService.Catalog] = CreateSubscription(DarkKitchenService.Catalog, CatalogQueue),
            [DarkKitchenService.Inventory] = CreateSubscription(
                DarkKitchenService.Inventory,
                InventoryQueue,
                KnownIntegrationEventTypes.OrderPlaced,
                KnownIntegrationEventTypes.RecipeChanged),
            [DarkKitchenService.Kds] = CreateSubscription(
                DarkKitchenService.Kds,
                KdsQueue,
                KnownIntegrationEventTypes.OrderAccepted,
                KnownIntegrationEventTypes.StationChanged,
                KnownIntegrationEventTypes.ProductStationRoutingChanged),
            [DarkKitchenService.OrderManagement] = CreateSubscription(
                DarkKitchenService.OrderManagement,
                OrderManagementQueue,
                KnownIntegrationEventTypes.InventoryReserved,
                KnownIntegrationEventTypes.InventoryReservationFailed,
                KnownIntegrationEventTypes.MenuItemChanged,
                KnownIntegrationEventTypes.ProductPriceChanged),
            [DarkKitchenService.Packing] = CreateSubscription(
                DarkKitchenService.Packing,
                PackingQueue,
                KnownIntegrationEventTypes.ItemPreparationCompleted,
                KnownIntegrationEventTypes.OrderReadyForPacking),
            [DarkKitchenService.Storefront] = CreateSubscription(
                DarkKitchenService.Storefront,
                StorefrontQueue,
                KnownIntegrationEventTypes.MenuItemChanged,
                KnownIntegrationEventTypes.ProductPriceChanged,
                KnownIntegrationEventTypes.InventoryReservationFailed,
                KnownIntegrationEventTypes.OrderAccepted,
                KnownIntegrationEventTypes.OrderReadyForPickup)
        };

    public static IReadOnlyCollection<IntegrationEventSubscription> Subscriptions { get; } =
        SubscriptionsByService.Values.ToArray();

    public static string ExchangeFor(string eventType)
    {
        if (!KnownIntegrationEventTypes.All.Contains(eventType))
        {
            throw new ArgumentOutOfRangeException(nameof(eventType), eventType, "Unknown integration event type.");
        }

        return $"{ExchangePrefix}.{eventType}";
    }

    public static string QueueFor(DarkKitchenService service)
    {
        return SubscriptionFor(service).QueueName;
    }

    public static IntegrationEventSubscription SubscriptionFor(DarkKitchenService service)
    {
        if (!SubscriptionsByService.TryGetValue(service, out var subscription))
        {
            throw new ArgumentOutOfRangeException(nameof(service), service, "Unknown Dark Kitchen service.");
        }

        return subscription;
    }

    public static IReadOnlyCollection<string> QueuesFor(string eventType)
    {
        if (!KnownIntegrationEventTypes.All.Contains(eventType))
        {
            throw new ArgumentOutOfRangeException(nameof(eventType), eventType, "Unknown integration event type.");
        }

        return Subscriptions
            .Where(subscription => subscription.EventTypes.Contains(eventType))
            .Select(subscription => subscription.QueueName)
            .ToArray();
    }

    private static IntegrationEventSubscription CreateSubscription(
        DarkKitchenService service,
        string queueName,
        params string[] eventTypes)
    {
        return new IntegrationEventSubscription(
            service,
            queueName,
            eventTypes.ToHashSet(StringComparer.Ordinal));
    }
}
