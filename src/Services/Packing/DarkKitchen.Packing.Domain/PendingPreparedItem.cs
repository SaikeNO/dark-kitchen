namespace DarkKitchen.Packing.Domain;

public sealed class PendingPreparedItem
{
    private PendingPreparedItem()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public Guid OrderItemId { get; private set; }
    public string StationCode { get; private set; } = string.Empty;
    public Guid CorrelationId { get; private set; }
    public string BrandId { get; private set; } = string.Empty;
    public DateTimeOffset CompletedAt { get; private set; }
    public DateTimeOffset ReceivedAt { get; private set; }

    public static PendingPreparedItem Create(
        Guid orderId,
        Guid orderItemId,
        string stationCode,
        Guid correlationId,
        string brandId,
        DateTimeOffset completedAt,
        DateTimeOffset receivedAt)
    {
        return new PendingPreparedItem
        {
            OrderId = orderId,
            OrderItemId = orderItemId,
            StationCode = stationCode,
            CorrelationId = correlationId,
            BrandId = brandId,
            CompletedAt = completedAt,
            ReceivedAt = receivedAt
        };
    }
}
