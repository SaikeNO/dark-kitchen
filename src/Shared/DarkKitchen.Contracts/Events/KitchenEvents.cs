namespace DarkKitchen.Contracts.Events;

public sealed record ItemPreparationStarted(
    Guid OrderId,
    Guid OrderItemId,
    string StationCode);

public sealed record ItemPreparationCompleted(
    Guid OrderId,
    Guid OrderItemId,
    string StationCode);
