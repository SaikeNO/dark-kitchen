namespace DarkKitchen.OrderManagement.Domain;

public sealed class Order
{
    private Order()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BrandId { get; private set; }
    public string ExternalOrderId { get; private set; } = string.Empty;
    public string SourceChannel { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public Guid CorrelationId { get; private set; }
    public decimal TotalPrice { get; private set; }
    public string Currency { get; private set; } = "PLN";
    public CustomerSnapshot? Customer { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public List<OrderItem> Items { get; private set; } = [];
    public List<OrderHistory> History { get; private set; } = [];

    public static Order Create(
        Guid brandId,
        string externalOrderId,
        string sourceChannel,
        Guid correlationId,
        CustomerSnapshot? customer,
        IEnumerable<OrderItem> items,
        DateTimeOffset now)
    {
        var orderItems = items.ToList();
        if (orderItems.Count == 0)
        {
            throw new ArgumentException("Order must contain at least one item.", nameof(items));
        }

        if (correlationId == Guid.Empty)
        {
            throw new ArgumentException("Correlation id is required.", nameof(correlationId));
        }

        var currency = orderItems[0].Currency;
        if (orderItems.Any(item => !string.Equals(item.Currency, currency, StringComparison.Ordinal)))
        {
            throw new ArgumentException("All order items must use the same currency.", nameof(items));
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            BrandId = RequireId(brandId, nameof(brandId)),
            ExternalOrderId = RequireNonWhiteSpace(externalOrderId, nameof(externalOrderId)),
            SourceChannel = RequireNonWhiteSpace(sourceChannel, nameof(sourceChannel)),
            Status = OrderStatus.Placed,
            CorrelationId = correlationId,
            TotalPrice = orderItems.Sum(item => item.LineTotal),
            Currency = currency,
            Customer = customer,
            CreatedAt = now,
            UpdatedAt = now,
            Items = orderItems
        };

        order.Customer?.AssignToOrder(order.Id);
        foreach (var item in order.Items)
        {
            item.AssignToOrder(order.Id);
        }

        order.History.Add(OrderHistory.Create(order.Id, null, OrderStatus.Placed, correlationId, now, "Order placed"));
        return order;
    }

    public bool Accept(Guid correlationId, DateTimeOffset now)
    {
        return MoveTo(OrderStatus.Accepted, correlationId, now, "Inventory reserved");
    }

    public bool Reject(string reasonCode, Guid correlationId, DateTimeOffset now)
    {
        if (IsTerminal || Status != OrderStatus.Placed)
        {
            return false;
        }

        return SetStatus(OrderStatus.Rejected, correlationId, now, reasonCode);
    }

    public bool MarkPreparing(Guid correlationId, DateTimeOffset now)
    {
        return MoveTo(OrderStatus.Preparing, correlationId, now, "Preparation started");
    }

    public bool MarkReadyForPacking(Guid correlationId, DateTimeOffset now)
    {
        return MoveTo(OrderStatus.ReadyForPacking, correlationId, now, "Ready for packing");
    }

    public bool MarkReadyForPickup(Guid correlationId, DateTimeOffset now)
    {
        return MoveTo(OrderStatus.ReadyForPickup, correlationId, now, "Ready for pickup");
    }

    private bool MoveTo(OrderStatus status, Guid correlationId, DateTimeOffset now, string reason)
    {
        if (IsTerminal || Status >= status)
        {
            return false;
        }

        return SetStatus(status, correlationId, now, reason);
    }

    private bool SetStatus(OrderStatus status, Guid correlationId, DateTimeOffset now, string reason)
    {
        if (correlationId == Guid.Empty)
        {
            throw new ArgumentException("Correlation id is required.", nameof(correlationId));
        }

        var previousStatus = Status;
        Status = status;
        UpdatedAt = now;
        History.Add(OrderHistory.Create(Id, previousStatus, status, correlationId, now, reason));
        return true;
    }

    private bool IsTerminal => Status is OrderStatus.Completed or OrderStatus.Rejected or OrderStatus.Cancelled;

    private static Guid RequireId(Guid id, string parameterName)
    {
        return id == Guid.Empty
            ? throw new ArgumentException($"{parameterName} is required.", parameterName)
            : id;
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
