namespace DarkKitchen.Contracts.Events;

public sealed record OrderPlaced(
    Guid OrderId,
    string SourceChannel,
    IReadOnlyList<OrderPlacedLine> Items);

public sealed record OrderPlacedLine(
    Guid OrderItemId,
    Guid MenuItemId,
    int Quantity);

public sealed record OrderAccepted(Guid OrderId);

public sealed record OrderReadyForPacking(Guid OrderId);

public sealed record OrderReadyForPickup(
    Guid OrderId,
    string PickupCode);
