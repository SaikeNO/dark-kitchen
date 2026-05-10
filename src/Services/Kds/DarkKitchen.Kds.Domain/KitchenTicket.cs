namespace DarkKitchen.Kds.Domain;

public sealed class KitchenTicket
{
    private readonly List<KitchenTask> _tasks = [];

    private KitchenTicket()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public Guid BrandId { get; private set; }
    public Guid CorrelationId { get; private set; }
    public string SourceChannel { get; private set; } = string.Empty;
    public KitchenTicketStatus Status { get; private set; } = KitchenTicketStatus.Open;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public IReadOnlyList<KitchenTask> Tasks => _tasks;

    public static KitchenTicket Create(
        Guid orderId,
        Guid brandId,
        Guid correlationId,
        string sourceChannel,
        DateTimeOffset now)
    {
        return new KitchenTicket
        {
            OrderId = orderId,
            BrandId = brandId,
            CorrelationId = correlationId,
            SourceChannel = sourceChannel,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void AddTask(KitchenTask task)
    {
        task.AssignTicket(this);
        _tasks.Add(task);
        RefreshStatus(DateTimeOffset.UtcNow);
    }

    public void RefreshStatus(DateTimeOffset now)
    {
        UpdatedAt = now;

        if (_tasks.Count == 0 || _tasks.Any(task => task.Status == KitchenTaskStatus.RoutingMissing))
        {
            Status = KitchenTicketStatus.RoutingIssues;
            return;
        }

        if (_tasks.All(task => task.Status == KitchenTaskStatus.Done))
        {
            Status = KitchenTicketStatus.Completed;
            return;
        }

        Status = _tasks.Any(task => task.Status == KitchenTaskStatus.InProgress)
            ? KitchenTicketStatus.InProgress
            : KitchenTicketStatus.Open;
    }
}
