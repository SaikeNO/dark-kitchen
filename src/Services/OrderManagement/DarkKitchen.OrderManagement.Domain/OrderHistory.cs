namespace DarkKitchen.OrderManagement.Domain;

public sealed class OrderHistory
{
    private OrderHistory()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public Order? Order { get; private set; }
    public OrderStatus? FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; }
    public string? Reason { get; private set; }
    public Guid CorrelationId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static OrderHistory Create(
        Guid orderId,
        OrderStatus? fromStatus,
        OrderStatus toStatus,
        Guid correlationId,
        DateTimeOffset now,
        string? reason)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("Order id is required.", nameof(orderId));
        }

        if (correlationId == Guid.Empty)
        {
            throw new ArgumentException("Correlation id is required.", nameof(correlationId));
        }

        return new OrderHistory
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            CorrelationId = correlationId,
            CreatedAt = now,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim()
        };
    }
}
