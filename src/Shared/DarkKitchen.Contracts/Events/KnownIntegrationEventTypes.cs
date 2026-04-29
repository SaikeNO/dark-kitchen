namespace DarkKitchen.Contracts.Events;

public static class KnownIntegrationEventTypes
{
    public const string OrderPlaced = "order.placed";
    public const string InventoryReserved = "inventory.reserved";
    public const string InventoryReservationFailed = "inventory.reservation_failed";
    public const string OrderAccepted = "order.accepted";
    public const string ItemPreparationStarted = "item.preparation_started";
    public const string ItemPreparationCompleted = "item.preparation_completed";
    public const string OrderReadyForPacking = "order.ready_for_packing";
    public const string OrderReadyForPickup = "order.ready_for_pickup";

    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        OrderPlaced,
        InventoryReserved,
        InventoryReservationFailed,
        OrderAccepted,
        ItemPreparationStarted,
        ItemPreparationCompleted,
        OrderReadyForPacking,
        OrderReadyForPickup
    };
}
