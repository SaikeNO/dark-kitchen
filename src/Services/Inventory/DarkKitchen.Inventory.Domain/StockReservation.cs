namespace DarkKitchen.Inventory.Domain;

public sealed class StockReservation
{
    private StockReservation()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public StockReservationStatus Status { get; private set; }
    public string? FailureReasonCode { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public List<StockReservationLine> Lines { get; private set; } = [];

    public static StockReservation Reserve(Guid orderId, IEnumerable<StockReservationLine> lines, DateTimeOffset now)
    {
        var reservationLines = lines.ToList();
        if (reservationLines.Count == 0)
        {
            throw new ArgumentException("Reservation must contain at least one line.", nameof(lines));
        }

        return new StockReservation
        {
            Id = Guid.NewGuid(),
            OrderId = RequireOrderId(orderId),
            Status = StockReservationStatus.Reserved,
            CreatedAt = now,
            Lines = reservationLines
        };
    }

    public static StockReservation Fail(Guid orderId, string reasonCode, DateTimeOffset now)
    {
        return new StockReservation
        {
            Id = Guid.NewGuid(),
            OrderId = RequireOrderId(orderId),
            Status = StockReservationStatus.Failed,
            FailureReasonCode = RequireNonWhiteSpace(reasonCode, nameof(reasonCode)),
            CreatedAt = now
        };
    }

    private static Guid RequireOrderId(Guid orderId)
    {
        return orderId == Guid.Empty
            ? throw new ArgumentException("Order id is required.", nameof(orderId))
            : orderId;
    }

    private static string RequireNonWhiteSpace(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }
}
