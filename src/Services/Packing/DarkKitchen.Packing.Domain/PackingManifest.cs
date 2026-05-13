namespace DarkKitchen.Packing.Domain;

public sealed class PackingManifest
{
    private readonly List<ManifestItem> _items = [];

    private PackingManifest()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public Guid BrandId { get; private set; }
    public Guid CorrelationId { get; private set; }
    public string SourceChannel { get; private set; } = string.Empty;
    public PackingManifestStatus Status { get; private set; } = PackingManifestStatus.Waiting;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ReadyForPackingAt { get; private set; }
    public DateTimeOffset? IssuedAt { get; private set; }
    public IReadOnlyList<ManifestItem> Items => _items;

    public int TotalItemsCount => _items.Count;
    public int ReadyItemsCount => _items.Count(item => item.IsReady);

    public static PackingManifest Create(
        Guid orderId,
        Guid brandId,
        Guid correlationId,
        string sourceChannel,
        DateTimeOffset now)
    {
        return new PackingManifest
        {
            OrderId = orderId,
            BrandId = brandId,
            CorrelationId = correlationId,
            SourceChannel = sourceChannel,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void AddItem(ManifestItem item)
    {
        item.AssignManifest(this);
        _items.Add(item);
        RefreshStatus(DateTimeOffset.UtcNow);
    }

    public bool MarkItemReady(Guid orderItemId, DateTimeOffset completedAt)
    {
        var item = _items.SingleOrDefault(candidate => candidate.OrderItemId == orderItemId);
        if (item is null || !item.MarkReady(completedAt))
        {
            return false;
        }

        RefreshStatus(completedAt);
        return true;
    }

    public bool MarkIssued(DateTimeOffset issuedAt)
    {
        if (Status == PackingManifestStatus.Issued)
        {
            return false;
        }

        if (Status != PackingManifestStatus.ReadyForPacking)
        {
            throw new InvalidOperationException("Manifest must be ready for packing before it can be issued.");
        }

        Status = PackingManifestStatus.Issued;
        IssuedAt = issuedAt;
        UpdatedAt = issuedAt;
        return true;
    }

    private void RefreshStatus(DateTimeOffset now)
    {
        UpdatedAt = now;

        if (Status == PackingManifestStatus.Issued)
        {
            return;
        }

        if (_items.Count > 0 && _items.All(item => item.IsReady))
        {
            Status = PackingManifestStatus.ReadyForPacking;
            ReadyForPackingAt ??= now;
            return;
        }

        Status = PackingManifestStatus.Waiting;
    }
}
