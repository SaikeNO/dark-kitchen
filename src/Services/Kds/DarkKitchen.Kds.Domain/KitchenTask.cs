namespace DarkKitchen.Kds.Domain;

public sealed class KitchenTask
{
    private KitchenTask()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TicketId { get; private set; }
    public KitchenTicket? Ticket { get; private set; }
    public Guid OrderItemId { get; private set; }
    public Guid MenuItemId { get; private set; }
    public string ItemName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public Guid? StationId { get; private set; }
    public string? StationCode { get; private set; }
    public KitchenTaskStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public static KitchenTask Create(
        Guid orderItemId,
        Guid menuItemId,
        string itemName,
        int quantity,
        Guid stationId,
        string stationCode,
        DateTimeOffset now)
    {
        return new KitchenTask
        {
            OrderItemId = orderItemId,
            MenuItemId = menuItemId,
            ItemName = itemName,
            Quantity = quantity,
            StationId = stationId,
            StationCode = stationCode,
            Status = KitchenTaskStatus.Pending,
            CreatedAt = now
        };
    }

    public static KitchenTask CreateRoutingMissing(
        Guid orderItemId,
        Guid menuItemId,
        string itemName,
        int quantity,
        DateTimeOffset now)
    {
        return new KitchenTask
        {
            OrderItemId = orderItemId,
            MenuItemId = menuItemId,
            ItemName = itemName,
            Quantity = quantity,
            Status = KitchenTaskStatus.RoutingMissing,
            CreatedAt = now
        };
    }

    public bool Start(DateTimeOffset now)
    {
        if (Status != KitchenTaskStatus.Pending)
        {
            return false;
        }

        Status = KitchenTaskStatus.InProgress;
        StartedAt = now;
        return true;
    }

    public bool Complete(DateTimeOffset now)
    {
        if (Status != KitchenTaskStatus.InProgress)
        {
            return false;
        }

        Status = KitchenTaskStatus.Done;
        CompletedAt = now;
        return true;
    }

    internal void AssignTicket(KitchenTicket ticket)
    {
        Ticket = ticket;
        TicketId = ticket.Id;
    }
}
