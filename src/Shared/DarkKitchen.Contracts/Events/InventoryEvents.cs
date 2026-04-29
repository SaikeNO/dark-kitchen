namespace DarkKitchen.Contracts.Events;

public sealed record InventoryReserved(
    Guid OrderId,
    Guid ReservationId);

public sealed record InventoryReservationFailed(
    Guid OrderId,
    string ReasonCode);
